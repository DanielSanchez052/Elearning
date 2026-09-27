# Feature: MVP3 — Sistema de Medallas (Badges)

## Objective
Implement the badge system per the locked design (see Engram memory "MVP3 Badges: final locked design"): 3 badges (LoginFirst, CourseDone, Speedster), the redefined Velocista mechanism (exam-start-to-submit timing, first attempt only), service-based award architecture in the monolith.

## Design reference
Full architecture resolved across two Opus-driven design review rounds (no code, pure design). All open questions resolved 2026-09-26. See Engram: "MVP3 Badges: final locked design (Velocista redefinition, all decisions resolved)".

## TDD mode
Strict TDD, real RED→GREEN — all of this is new production code, no exceptions.

## Delivery strategy
`ask-on-risk`. Each track below is its own branch/PR (independent of the others until the Integration track). Within a track, each task is its own commit.

---

## Track A — Exam timing mechanism (independent, shippable alone)
Gives accurate quiz-duration data; useful even before badges exist (feeds future "tiempo promedio" reports per alcance_elearning.md §3.6).

- [x] A1. Domain: `ExamSession` entity (Id, UserId, CourseId, AttemptNumber, StartedAt, SubmittedAt?, `Start()`/`MarkSubmitted()`/`IsOpen`). `UserQuizResult`: add nullable `StartedAt` + `Duration` helper. `CourseEnrollment.TryComplete()`: guard against re-completing (bugfix, unrelated to badges but needed here since we're touching this file). **Complexity: M.**
- [x] A2. Infrastructure: `ExamSessionConfiguration` (unique `(UserId,CourseId,AttemptNumber)`; partial unique `(UserId,CourseId) WHERE SubmittedAt IS NULL`). `UserQuizResultConfiguration`: add `started_at`. `ApplicationDbContext.ExamSessions`. `IQuizRepository`/`QuizRepository`: `GetOpenExamSessionAsync`, `TryAddExamSessionAsync` (catch Postgres `23505`). Migration `AddExamSessions`. **Complexity: M** (two unique indexes, one partial — verify EF/Npgsql syntax).
- [x] A3. Application: `StartCourseExamCommand`+handler (mirrors `GetCourseExamHandler`'s checks: enrolled/active/required-lessons/has-questions/not-already-passed/attempts-remain; returns existing open session unchanged if one exists, else creates attempt N+1). Modify `SubmitQuizHandler`: look up open session by `(UserId,CourseId)`, stamp `StartedAt` onto the result, close the session, capture the previously-discarded `bool` from `TryComplete(...)` — **no new field on `SubmitQuizCommand`**. Backward-compatible: no session found → submit as today with `StartedAt = null`. **Complexity: L** (touches the app's most complex handler).
- [x] A4. API: `POST /api/quizzes/courses/{courseId}/exam/start` on `QuizzesController`. **Complexity: S.**
- [x] A5. Frontend: `QuizSessionPage.tsx` calls the start endpoint (not plain GET) for course exams; retry calls start again; **adjust the countdown formula** so it's consistent with the 10-min Speedster threshold for short exams too (currently `max(120s, 45s×questions)` under-shoots it); ideally drive the displayed countdown from the server's `startedAt` so a reload doesn't reset it. **Complexity: M.**
- [x] A6. Tests: `StartCourseExamHandlerTests`, `SubmitQuizHandlerTests` additions (session stamping, stale-session Conflict, no-session backward-compat), Domain tests for `ExamSession`/`Duration`/`TryComplete` guard. **Complexity: L** (most of this track's real coverage).

## Track B — Badge core (independent of Track A, except Speedster rule)
- [x] B1. Domain: trim `BadgeCode` to `LoginFirst, CourseDone, Speedster`. `UserBadge`: add nullable `CourseId`. `IBadgeRepository`: `GetByCodeAsync`, `GetAllAsync`, `GetByUserAsync`, `HasBadgeAsync(userId,badgeId,courseId?)`, `TryAddAsync` (bool), `SaveChangesAsync`. **Complexity: S.**
- [x] B2. Infrastructure: `UserBadgeConfiguration` — add `course_id` + FK to `courses` (Restrict), drop `IX_user_badges_UserId_BadgeId`, add `NULLS NOT DISTINCT` unique index on `(UserId,BadgeId,CourseId)` (Postgres 17, confirmed supported). `BadgeConfiguration`: seed the 3 badge rows (`HasData`, Spanish name/description). `BadgeRepository` impl (23505 catch in `TryAddAsync`). Migration `AddCourseScopeToUserBadgesAndSeedBadges`. **Complexity: M.**
- [x] B3. Application: `IBadgeAwardService`/`BadgeAwardService` (`OnUserLoggedInAsync`, `OnCourseCompletedAsync`, `OnCourseExamPassedAsync` — this 3rd method's real logic depends on Track A's `Duration`, stub/skip until Integration). Rule interfaces (`ILoginBadgeRule`, `ICourseCompletionBadgeRule`, `IExamPassedBadgeRule`). `FirstLoginRule`, `CourseCompletedRule` (Speedster is Integration, needs Track A). Explicit DI registration (reflection scan only covers handlers/validators). **Complexity: M.**
- [x] B4. Application: `GetMyBadgesQuery`+handler (earned badges, per-course title where applicable). `BadgesController`: `GET /api/badges/me`. **Complexity: S.**
- [x] B5. Tests: `BadgeAwardServiceTests` (already-owned → no insert; race via `TryAddAsync=false` → no crash; unseeded badge code → handled gracefully, not an unhandled exception; success → returned in result list), `FirstLoginRuleTests`, `CourseCompletedRuleTests`, `GetMyBadgesHandlerTests`. **Complexity: M.**

## Track C — Notifications plumbing for badge-earned (small, independent)
- [x] C1. `INotificationRepository.AddAsync` + implementation (currently missing — the existing Notifications CRUD never needed to create rows from server-side code before). **Complexity: S.**
- [x] C2. `INotificationPublisher` (Application/Features/Notifications/Services) — stages the in-app row now; email/SignalR channels are no-ops/deferred for this slice (see design decision: no email, no toast contract change yet), built as a reusable seam for the other 2 MVP3 notification triggers later. **Complexity: S.**
- [x] C3. Tests: `NotificationPublisherTests`. **Complexity: S.**

## Integration — depends on A + B + C all being done
- [ ] I1. `SpeedsterRule` (needs Track A's `UserQuizResult.Duration` + Track B's rule infra): awards `Speedster` scoped to `CourseId` when `CourseId != null && IsPassed && Duration < 10min && AttemptNumber == 1`.
- [ ] I2. Wire `BadgeAwardService` into the 3 handlers: `LoginHandler` → `OnUserLoggedInAsync` after successful login. `MarkLessonCompleteHandler` → `OnCourseCompletedAsync` when its (already-captured) `TryComplete` result is true. `SubmitQuizHandler` → `OnCourseCompletedAsync` (from the now-captured `TryComplete` bool) AND `OnCourseExamPassedAsync` (only for course-exam path). All calls best-effort, after the handler's own `SaveChangesAsync`, never fail the user's action.
- [ ] I3. Wire `INotificationPublisher` into `BadgeAwardService` so every award also creates the in-app notification.
- [ ] I4. Update the 3 handler test files to verify badge-service calls (`Times.Once`/`Times.Never` per branch) without re-testing rule logic itself.
- [ ] I5. Frontend: wire `Profile.tsx`'s "Badges" section (currently the intentional static placeholder) to a new `useMyBadges()` hook + `api/badges.ts`, same pattern as the certificates/enrollments sections built earlier this session.
- [ ] I6. Full backend suite green, `npx tsc -b --noEmit` clean, manual browser smoke test (log in as student, complete a course fast enough to trigger both badges, confirm they show in Profile and the notification bell).

## Docs (handled directly, not delegated — small doc edits)
- [ ] Update `docs/arquitectura_elearning_v2.md`: record the monolith-not-microservice decision.
- [ ] Update `docs/alcance_elearning.md` §3.4.1: replace the old wall-clock Velocista description with the exam-timing redefinition.

## Follow-ups (non-blocking, from Gentle AI review of Track A — review-reliability lens, approved 2026-09-27)
Neither finding opened a correction or blocks delivery. Tracked here to revisit later, not urgent.
- [ ] **R3-001** — `QuizRepository.TryAddExamSessionAsync` (`src/backend/ELearning.Infrastructure/Repositories/QuizRepository.cs:169-184`): the concurrency guard (catch Postgres `23505` from the partial unique index, detach the rejected entity) has no automated test — only verified once manually via a rolled-back Postgres transaction. Existing handler tests mock this repository, so nothing proves the catch filter matches the real exception shape, that a detached entity isn't re-sent on a later `SaveChanges`, or that the filtered index really allows only one open session. Would need an integration test against a real Postgres instance (this repo has no integration test project yet).
- [ ] **R3-002** — `QuizSessionPage.tsx:300-311`: since the course-exam page now loads questions only via the `POST .../exam/start` endpoint, a student who already passed or already used all attempts gets `StartCourseExamCommand`'s `ValidationFailure` and sees only an error message + "Volver al curso" — before this track, the page could still show the previous result/retry state via the read-only `GET`. Worth restoring: on a "ya aprobaste"/"sin intentos" error from start, fall back to showing the existing exam-results query instead of a bare error.
- [ ] **R3-003** (found on the re-review after the follow-ups doc commit) — `StartCourseExamCommand.cs:83-107`: `TryAddExamSessionAsync` returns `false` for a unique-violation on EITHER index (the one-open-session partial index, or the `(user,course,attempt_number)` index), not just the open-session race it was meant to guard. If a CLOSED session already exists for the computed `attemptNumber` (e.g. results were reset/deleted, or the stale-session-close branch closed a session at a higher attempt number than expected), the insert fails on the attempt index, `GetOpenExamSessionAsync` finds nothing to return, and every subsequent `Start` call returns `Conflict` — permanently locking the student out of that course's exam. Unlikely in normal flow, but a real edge case with no test coverage. Would need `TryAddExamSessionAsync` to distinguish which index was violated (e.g. inspect the Postgres constraint name in the exception) and handle the attempt-index case differently (probably: re-derive the correct next attempt number and retry once, rather than surfacing Conflict).

## Progress
(filled in as completed)

### Track A — Exam timing (branch `feature/mvp3-exam-timing`, from `main`, not pushed)
Route: delegated direct, a single writer for the whole track. TDD: strict (xUnit + Moq, `dotnet test ELearning.Tests/ELearning.Tests.csproj` from `src/backend`). Frontend has no test runner, so it was checked with `npx tsc -b --noEmit` and eslint.

| Task | Commit | Tests (full suite) |
|------|--------|--------------------|
| A1 Domain | `0e905e7` | 541 → 553 (+12: ExamSession 5, UserQuizResult 4, CourseEnrollment 3) |
| A2 Infrastructure + migration | `3afaf68` | 553 (no unit-testable surface; migration SQL verified on Postgres 17, see below) |
| A3 Application | `d51d9b4` | 553 → 575 (+15 StartCourseExamHandlerTests, +7 SubmitQuizHandlerTests) |
| A4 API | `013e97e` | 575 (no controller test project; route smoke-checked: 401 unauthenticated vs 404 for a bogus sibling) |
| A5 Frontend (+ `ServerNow` on the start DTO) | `054a1d4` | 575 → 576 (+1) |
| A6 Edge-case tests + this entry | this commit | 576 → 580 (+4) |

- `SubmitQuizHandlerTests` filtered run: **27/27 before** (the pre-existing count is 27, not 35), **36/36 after** (27 original, unchanged and passing, + 9 new).
- Full suite: **541 → 580**, 0 failures. `npx tsc -b --noEmit`: clean. ESLint on touched files: same 3 errors + 1 warning as before the change (all pre-existing in `QuizSessionPage.tsx`), none new.
- The A6 additions are characterization tests. They passed on first run because A1/A3 already had the behaviour; every earlier test followed real RED→GREEN.

**Migration** `20260927003351_AddExamSessions`. Generated SQL (`dotnet ef migrations script`):
```sql
ALTER TABLE user_quiz_results ADD started_at timestamp with time zone;
CREATE TABLE exam_sessions (id uuid PK, user_id uuid FK users CASCADE, course_id uuid FK courses CASCADE,
  attempt_number integer NOT NULL, started_at timestamptz NOT NULL, submitted_at timestamptz NULL);
CREATE UNIQUE INDEX idx_exam_session_one_open_per_user_course ON exam_sessions (user_id, course_id) WHERE submitted_at IS NULL;
CREATE UNIQUE INDEX idx_exam_session_user_course_attempt ON exam_sessions (user_id, course_id, attempt_number);
CREATE INDEX "IX_exam_sessions_course_id" ON exam_sessions (course_id);
```
It was verified against the local Postgres 17.0 dev DB **inside a rolled-back transaction**: the DDL applied, a second open session and a duplicate attempt were both rejected with SqlState 23505 on the expected index, and `exam_sessions` did not exist after the rollback. The migration is **not applied** to the dev DB, because other track branches share it. Run `dotnet ef database update` when this track merges. `dotnet-ef` was already 10.0.0 globally, so no bump was needed. The model snapshot was in sync, so the migration contains only this track's changes.

**Backward compatibility (SubmitQuizHandler)**
- Lesson-quiz path: untouched. `GetOpenExamSessionAsync` is never called, and `StartedAt` stays null.
- Course-exam path with no open session (an older client that never called `/exam/start`): identical to before, with `StartedAt = null`.
- Course-exam path with an open session whose `AttemptNumber` matches: `StartedAt` is stamped onto the result, and the session is closed in the same single `SaveChangesAsync` (the session is tracked by the shared DbContext).
- Open session for a different attempt: returns `Conflict` ("Tu sesión de examen no corresponde al intento actual. Vuelve a iniciar el examen.") after the attempt-rule checks and **before** any scoring or persistence.
- `GET /quizzes/courses/{id}/exam` is unchanged (it still neither starts the clock nor uses an attempt).

**StartCourseExam behaviour.** It uses the same checks as GetCourseExamHandler (Forbidden for not enrolled, inactive, or missing required lessons; NotFound when there are no questions) plus SubmitQuizHandler's attempt rules (Validation for "Ya aprobaste" and "máximo de N intentos"). An open session for the current attempt is returned unchanged. Otherwise attempt `(latest?.AttemptNumber ?? 0) + 1` is created.
- **Deviation:** an open session left behind for an *outdated* attempt is closed (`MarkSubmitted`, saved), and a fresh session is created. Without this, the stale-session `Conflict` in Submit would lock the student out for good, because Start would keep handing the stale session back.
- A concurrent start (`TryAddExamSessionAsync` = false) re-reads the open session and returns it; if none is found, it returns `Conflict`.

**TryComplete bool, for Integration.** It is captured as `courseCompleted` in `SubmitQuizHandler` and exposed as `QuizResultDto.CourseCompleted` (a trailing optional record parameter, default `false`, so no existing constructor call changed). Integration I2 can read `courseCompleted` in the handler after `SaveChangesAsync` to call `OnCourseCompletedAsync`. The frontend type has `courseCompleted?: boolean`. `CourseEnrollment.TryComplete` now returns `false` without touching `CompletedAt` when the enrollment is already completed. This is an incidental correctness fix: the IsActive gates already prevented it in both handlers, and Velocista reads `UserQuizResult.Duration`, not enrollment timestamps.

**Frontend timer.**
- Course exams now use `max(15 min, 60 s × questions)`; lesson quizzes keep `max(120 s, 45 s × questions)`.
- Reasoning: the auto-submit must never land inside the 10-minute Velocista window. Otherwise every pass of a short exam (the old formula gave ≤ 585 s for ≤ 13 questions) would earn the badge by construction. The 15-minute floor keeps a 5-minute margin even for tiny exams, so finishing under 10 minutes is always a real choice. 60 s per question lets large exams grow past the floor.
- The countdown is driven by the server: `remaining = duration − (serverNow − startedAt)`, where `ServerNow` was added to `StartCourseExamResultDto` so device clock skew cannot affect the result. It then follows a wall-clock deadline in `quizSessionStore`, so reloads and throttled background tabs do not reset or stretch it.
- Start is a React Query *mutation*, not a query, so refetch or invalidation never opens a new attempt. Retry calls start again.
- A resumed attempt whose countdown already expired is **not** auto-submitted. The page shows a notice and the student submits manually, because the design says 10 minutes is not a hard cutoff and abandoned attempts must not use up an attempt.

**Not done / notes**
- No browser smoke test: the start endpoint needs the unapplied migration.
- Pre-existing quirk left alone: `SubmitQuizHandler` picks the result branch with `cmd.LessonId.HasValue`, while the other branches use `HasValue && != Guid.Empty` (unreachable from the controllers).

Two review rounds by Gentle AI (review-reliability lens) on this track, both **approved** — see the Follow-ups section above for the 3 non-blocking findings (R3-001, R3-002, R3-003).

### Track C — Notifications plumbing (done, 2026-09-26)
- C1 (`d2a1af5`): `INotificationRepository.AddAsync(Notification, CancellationToken)` added + implemented in `NotificationRepository` (stages via `_db.Notifications.AddAsync`, does not save). No dedicated repo test — matches this codebase's existing convention (no repository-level tests anywhere; repos are exercised via handler-test mocks).
- C2 (`0b6b534`): `INotificationPublisher`/`NotificationPublisher` added under `ELearning.Application/Features/Notifications/Services/`. Registered explicitly in `DependencyInjectionExtensions.AddApplication()` via a new `RegisterServices` step (not covered by the reflection-based handler/validator scan). Signature settled on:
  ```csharp
  Task PublishAsync(Guid userId, NotificationType type, string title, string message, Guid? referenceId = null, CancellationToken ct = default);
  ```
  Generic enough to serve badge-earned now and the other 2 MVP3 triggers (new course published, pending-course reminder) later with no signature change.
- C3 (this commit): `NotificationPublisherTests` (3 tests) — verifies `AddAsync` is called with a correctly-populated `Notification` (UserId/Type/Title/Message/ReferenceId), verifies a null `referenceId` round-trips, and **verifies `SaveChangesAsync` is never called** (`Times.Never`) to lock in the caller-saves contract.
- **Confirmed: `NotificationPublisher` does NOT call `SaveChangesAsync`.** Email and SignalR are explicitly NOT wired — only the in-app row is staged. Integration (a later track) must call `SaveChangesAsync` itself (e.g. as part of `BadgeAwardService`'s existing save) for the notification to persist.
- Test counts: C1 → 0 new (no repo test convention in this repo), C2 → 0 new (implementation-only commit), C3 → 3 new (`NotificationPublisherTests`).
- Full backend suite: 544 passed, 0 failed, 0 skipped (541 baseline + 3 new). No regressions.

### Track B — Badge core (done, 2026-09-26)

Commits (feature/mvp3-badges-core):
- B1 `2075f7f` — Domain: trimmed `BadgeCode`, `UserBadge.CourseId`, extended `IBadgeRepository`. 4 tests added (`UserBadgeEntityTests`).
- B2 `9b217d6` — Infrastructure: `UserBadgeConfiguration`/`BadgeConfiguration`/`BadgeRepository`, migration `20260927000542_AddCourseScopeToUserBadgesAndSeedBadges`. 0 dedicated tests (no repository in this codebase has dedicated unit tests — no integration test project exists); validated via full-suite green (545/545, no regressions).
- B3 `fc66953` — Application: `IBadgeAwardService`/`BadgeAwardService`, `ILoginBadgeRule`/`ICourseCompletionBadgeRule`/`IExamPassedBadgeRule` (contract only), `FirstLoginRule`, `CourseCompletedRule`, DI wiring. Tests committed in B5 (task breakdown assigns all Gamification tests to B5).
- B4 `79f7eda` — Application/API: `GetMyBadgesQuery`+handler, `BadgesController` (`GET /api/badges/me`). Tests committed in B5.
- B5 (this commit) — Tests: `BadgeAwardServiceTests` (7), `FirstLoginRuleTests` (1), `CourseCompletedRuleTests` (1), `GetMyBadgesHandlerTests` (5) = 14 tests, plus `GamificationTestHelpers` (reflection-based builders, mirrors `CertificateTestHelpers`). Also updates these checkboxes/progress notes.

Test count: 4 (B1) + 0 (B2) + 14 (B5, covering B3+B4) = **18 tests added**, full suite 541 → 559, all green.

Migration `AddCourseScopeToUserBadgesAndSeedBadges`: drops `IX_user_badges_UserId_BadgeId`, adds `course_id` (nullable uuid) + `FK_user_badges_courses_course_id` (`ON DELETE RESTRICT`), seeds 3 badge rows (ids 1-3), creates `IX_user_badges_UserId_BadgeId_course_id` as `UNIQUE ... NULLS NOT DISTINCT`. Confirmed via `dotnet ef migrations script` that the generated SQL contains `NULLS NOT DISTINCT` (Postgres 17). `AreNullsDistinct(false)` on `IndexBuilder` compiled and worked as-is with `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.0 / EF Core 10.0.0 — no fallback syntax needed.

Deviations from the brief (with reasoning):
1. **BadgeRepository stub in the B1 commit.** B1 is scoped to Domain, but `IBadgeRepository` gaining new members would leave the existing empty `BadgeRepository : IBadgeRepository` failing to compile. Added `NotImplementedException`-throwing stubs for the new members in the B1 commit so the solution keeps building; B2 replaces them with the real EF implementation. Noted so it's not mistaken for scope creep.
2. **B3/B4 test files committed in B5, not alongside their implementation.** The task's own numbered breakdown assigns `BadgeAwardServiceTests`/`FirstLoginRuleTests`/`CourseCompletedRuleTests`/`GetMyBadgesHandlerTests` to B5, authored *after* B3/B4's production code. Genuine test-first RED was only possible for B4 (query/handler didn't exist yet — confirmed CS0234/CS0246 RED, then GREEN 5/5). For B3, the tests were written and passed against already-existing implementation (no RED phase for that specific code), which is disclosed rather than fabricated.
3. **`AwardedBadgeDto` created in B3, not B4.** The brief places both DTOs in a `BadgeDtos.cs` file under B4, but `IBadgeAwardService`'s B3 method signatures need `AwardedBadgeDto` to compile. Created the file in B3 with just `AwardedBadgeDto`; B4 added `UserBadgeDto` to the same file.
4. **`IExamPassedBadgeRule` defined but not referenced anywhere.** Per the brief's own "your call" — defined the contract (documents the seam for Integration's `SpeedsterRule`) but did NOT inject it into `BadgeAwardService`'s constructor, since nothing implements it yet and requiring an unregistered interface would break DI at startup. `OnCourseExamPassedAsync` is a plain stub with a `// TODO(Track A)` comment, not wired to the interface.
5. **No notification wiring in `BadgeAwardService`.** Track C doesn't exist on this branch; the brief flagged this as expected and left the call to my judgment. Did not reference `INotificationPublisher` at all — Integration will need to add that call once Track C lands.
6. **`dotnet-ef` global tool bumped 9.0.1 → 10.0.0** before generating the migration, to match the installed `Microsoft.EntityFrameworkCore`/`Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.0 packages.

Out of scope, confirmed untouched: `ExamSession`, `StartCourseExamCommand`, `SubmitQuizHandler`, notification publisher, `LoginHandler`/`MarkLessonCompleteHandler` wiring, `SpeedsterRule`.

Next step: Integration as planned. Integration I1 reads `UserQuizResult.Duration` + `AttemptNumber == 1`; I2 reads the captured `courseCompleted` (Track A) and calls `IBadgeAwardService` (Track B) + `INotificationPublisher` (Track C).
