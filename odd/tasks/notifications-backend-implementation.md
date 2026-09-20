# Feature: Implement Notifications backend (fix active production bug)

## Objective
`NotificationBell` is rendered unconditionally in `AppHeader.tsx` and polls `GET /notifications` every 30s via `useNotifications`. All 4 backend handlers (`GetMyNotificationsHandler`, `GetUnreadNotificationsCountHandler`, `MarkNotificationAsReadHandler`, `MarkAllNotificationsAsReadHandler`) unconditionally `throw new NotImplementedException()`, `INotificationRepository` is empty, `NotificationRepository` is an empty impl, and `NotificationsController` has zero endpoints. Every logged-in user is currently getting a failed request every 30 seconds.

## Why
Identified while ordering remaining MVP2 work by impact: this is the only item that is actively broken right now (not just "not yet built") — user-facing, global, recurring.

## Scope
1. `ELearning.Domain/Interfaces/Repositories/INotificationRepository.cs` — currently `public interface INotificationRepository { }` (empty). Add methods needed by the 4 handlers (get by id, get by user ordered by CreatedAt desc, unread count by user, save changes; mark-all-as-read can be read-then-loop-then-save, doesn't need a dedicated bulk method unless it's cleaner in EF).
2. `ELearning.Infrastructure/Repositories/NotificationRepository.cs` — currently `public class NotificationRepository : INotificationRepository { }` (empty). Implement against `ApplicationDbContext.Notifications` (`DbSet<Notification>` already exists, table already migrated).
3. `ELearning.Application/Features/Notifications/Commands/NotificationCommands.cs` — `MarkNotificationAsReadCommand`/`MarkAllNotificationsAsReadCommand` currently carry no `UserId`. Every other handler in this codebase takes `UserId` explicitly (set by the controller from `User.GetUserId()`, see `QuizzesController` for the pattern) — add `UserId` to both commands and implement the handlers for real.
4. `ELearning.Application/Features/Notifications/Queries/NotificationQueries.cs` — same: `GetMyNotificationsQuery`/`GetUnreadNotificationsCountQuery` need `UserId` added, then implement.
5. `ELearning.API/Controllers/NotificationsController.cs` — currently empty, no `[Authorize]`, no actions. Add `[Authorize]` at class level (matches `QuizzesController`/other controllers) and wire endpoints matching what the frontend already calls, plus the two handlers the frontend doesn't call yet:
   - `GET /notifications` → `GetMyNotificationsQuery` (frontend already calls this path)
   - `PUT /notifications/{id}/mark-read` → `MarkNotificationAsReadCommand` (frontend already calls this path — note the frontend uses `PUT`, keep that verb)
   - `GET /notifications/unread-count` → `GetUnreadNotificationsCountQuery` (not yet called by frontend, but the handler exists for a reason — expose it)
   - `PUT /notifications/mark-all-read` → `MarkAllNotificationsAsReadCommand` (same — expose it even though unused today)
   - Do NOT build an endpoint for `notificationsApi.createNotification` — verified it is dead code, called from nowhere in the frontend, and there is no corresponding Application command. Out of scope.
6. **Security-relevant**: `MarkNotificationAsReadHandler` MUST verify `notification.UserId == command.UserId` before marking as read (return `NotFound` or `Forbidden` — match this codebase's `Result`/`ResultErrorType` convention, see other handlers for which error type they use when a resource exists but belongs to someone else) — this was flagged as a real risk during the earlier test-coverage pass (any authenticated user could otherwise mark/read arbitrary notifications by guessing IDs). `MarkAllNotificationsAsReadHandler` only touches notifications scoped to `command.UserId`, never all rows in the table.
7. **Frontend DTO contract mismatch** (must fix or the bell will misbehave once the backend responds): `src/frontend/elearning-web/src/api/notifications.ts` declares `NotificationDto { id, message, read }`, but the real backend `NotificationDto` (`ELearning.Application/Features/Notifications/DTOs/NotificationDtos.cs`) serializes as `{ id, title, message, type, isRead, createdAt }` (ASP.NET Core default camelCase). Update the frontend interface to match exactly, and update every place that reads `.read` (`NotificationBell.tsx` line 5: `n.read` → `n.isRead`; check `useNotifications.ts` and any other consumer via grep for `.read` on a notification object — don't miss one and leave a silent bug).

## Constraints / conventions
- Backend: xUnit + Moq, mirror `ELearning.Tests/Unit/Aplication/Features/Notifications/<HandlerName>Tests.cs` (new folder). Pattern reference: `ELearning.Tests/Unit/Aplication/Features/Enrollments/EnrollInCourseHandlerTests.cs`.
- Controller pattern reference: `ELearning.API/Controllers/QuizzesController.cs` (constructor-injected handlers, `User.GetUserId()`, `this.ToActionResult(result)`).
- Keep the existing class-based style for the 4 Commands/Queries (don't rewrite them as records) — minimize unrelated diff noise.
- No changes to the `Notification` domain entity — it's already complete (`Create`, `MarkAsRead`, `MarkAsUnread`).

## TDD mode
Strict TDD enabled globally, and this IS new production code (unlike the earlier characterization-testing pass) — real RED→GREEN applies:
- RED: write the handler test first: with the stub still throwing `NotImplementedException`, `dotnet test --filter "FullyQualifiedName~Notifications"` must show real failures (the exception itself is valid RED evidence).
- GREEN: implement the handler/repository, re-run, confirm pass.
- Runner: `dotnet test ELearning.Tests/ELearning.Tests.csproj` from `src/backend`.
- Capture both the RED and GREEN run outputs as evidence in the Progress section, not just the final GREEN.

## Delivery strategy
Single work unit — this is one cohesive vertical slice (repo + handlers + controller + frontend contract fix), splitting it would just add commit-boundary overhead without real independence. One commit, Conventional Commits, no AI attribution (user's global rule). Branch: continue on `test/quizzes-notifications-reports-coverage` unless that branch is inappropriate for non-test work — if so, branch from it as `feature/notifications-implementation`.

## Tasks
- [x] 1. Repository interface + EF implementation
- [x] 2. Application handlers (4) with UserId added to commands/queries, ownership check on mark-as-read
- [x] 3. Controller endpoints wired with `[Authorize]`
- [x] 4. Backend handler tests (RED then GREEN, captured)
- [x] 5. Frontend DTO contract fix (`notifications.ts`, `NotificationBell.tsx`, any other `.read` consumer)

## Progress

**TDD cycle** — strict mode, real RED→GREEN (this is new production code, not characterization tests).

Mechanical/structural step first (not "implementation"): added `UserId` to `MarkNotificationAsReadCommand`, `MarkAllNotificationsAsReadCommand`, `GetMyNotificationsQuery`, `GetUnreadNotificationsCountQuery`. Also converted the 4 handler stubs from explicit-interface-implementation with no constructor to the codebase's standard constructor-injected public-method shape (still `throw new NotImplementedException()`), and added the `INotificationRepository` method signatures + a real EF `NotificationRepository` implementation — both needed so `Mock<INotificationRepository>` and `new XxxHandler(mock.Object)` would even compile for the tests. None of this touches handler business logic.

**RED** — `dotnet test ELearning.Tests/ELearning.Tests.csproj --filter "FullyQualifiedName~Notifications"` with handlers still throwing:
```
Con error! - Con error:    10, Superado:     0, Omitido:     0, Total:    10, Duración: 154 ms - ELearning.Tests.dll (net10.0)
```
All 10 failures were `System.NotImplementedException : The method or operation is not implemented.` at each handler's `HandleAsync`.

**GREEN** — implemented the 4 handlers (with the ownership check in `MarkNotificationAsReadHandler`: `NotFound` when the id doesn't exist, `Forbidden` when `notification.UserId != command.UserId`) and re-ran the same filtered command:
```
Correctas! - Con error:     0, Superado:    10, Omitido:     0, Total:    10, Duración: 150 ms - ELearning.Tests.dll (net10.0)
```

**Full suite** (`dotnet test ELearning.Tests/ELearning.Tests.csproj`, no filter) — confirms no regressions elsewhere:
```
Correctas! - Con error:     0, Superado:   535, Omitido:     0, Total:   535, Duración: 476 ms - ELearning.Tests.dll (net10.0)
```
535 total, up from 525 before this change (10 new Notifications tests added).

**Frontend fix** — `src/frontend/elearning-web/src/api/notifications.ts` `NotificationDto` now matches the real backend shape (`{ id, title, message, type, isRead, createdAt }`); `NotificationBell.tsx` now reads `n.isRead` instead of `n.read`. Grepped the whole frontend `src/` for `.read`/`notification` usage: the only consumer reading `.read` off the API DTO was `NotificationBell.tsx` (fixed). `useNotifications.ts` itself doesn't touch `.read` directly — it just passes query data through, so no change needed there. Found two unrelated files that also declare a `read: boolean` shape but are **not** wired to the backend API at all (not imported anywhere in the app): `src/store/notificationStore.ts` (a standalone Zustand toast store) and `src/types/notification.types.ts` (an unused type file). Left both untouched — out of scope, and touching them wouldn't fix or break anything since nothing consumes them.

No frontend test runner is configured (`package.json` has `dev`/`build`/`lint`/`preview` only, no `vitest`/`jest`/`test` script) — did not invent one. Ran `npx tsc -b --noEmit` instead as a compile-level sanity check after the DTO change: no errors.

**Ownership check verified in tests**: `MarkNotificationAsReadHandlerTests.HandleAsync_NotificationBelongsToAnotherUser_ReturnsForbiddenAndDoesNotSave` asserts `ResultErrorType.Forbidden`, that `notification.IsRead` stays `false`, and that `SaveChangesAsync` is never called when a different user's `UserId` is used against someone else's notification.
