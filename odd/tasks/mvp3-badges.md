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

- [ ] A1. Domain: `ExamSession` entity (Id, UserId, CourseId, AttemptNumber, StartedAt, SubmittedAt?, `Start()`/`MarkSubmitted()`/`IsOpen`). `UserQuizResult`: add nullable `StartedAt` + `Duration` helper. `CourseEnrollment.TryComplete()`: guard against re-completing (bugfix, unrelated to badges but needed here since we're touching this file). **Complexity: M.**
- [ ] A2. Infrastructure: `ExamSessionConfiguration` (unique `(UserId,CourseId,AttemptNumber)`; partial unique `(UserId,CourseId) WHERE SubmittedAt IS NULL`). `UserQuizResultConfiguration`: add `started_at`. `ApplicationDbContext.ExamSessions`. `IQuizRepository`/`QuizRepository`: `GetOpenExamSessionAsync`, `TryAddExamSessionAsync` (catch Postgres `23505`). Migration `AddExamSessions`. **Complexity: M** (two unique indexes, one partial — verify EF/Npgsql syntax).
- [ ] A3. Application: `StartCourseExamCommand`+handler (mirrors `GetCourseExamHandler`'s checks: enrolled/active/required-lessons/has-questions/not-already-passed/attempts-remain; returns existing open session unchanged if one exists, else creates attempt N+1). Modify `SubmitQuizHandler`: look up open session by `(UserId,CourseId)`, stamp `StartedAt` onto the result, close the session, capture the previously-discarded `bool` from `TryComplete(...)` — **no new field on `SubmitQuizCommand`**. Backward-compatible: no session found → submit as today with `StartedAt = null`. **Complexity: L** (touches the app's most complex handler).
- [ ] A4. API: `POST /api/quizzes/courses/{courseId}/exam/start` on `QuizzesController`. **Complexity: S.**
- [ ] A5. Frontend: `QuizSessionPage.tsx` calls the start endpoint (not plain GET) for course exams; retry calls start again; **adjust the countdown formula** so it's consistent with the 10-min Speedster threshold for short exams too (currently `max(120s, 45s×questions)` under-shoots it); ideally drive the displayed countdown from the server's `startedAt` so a reload doesn't reset it. **Complexity: M.**
- [ ] A6. Tests: `StartCourseExamHandlerTests`, `SubmitQuizHandlerTests` additions (session stamping, stale-session Conflict, no-session backward-compat), Domain tests for `ExamSession`/`Duration`/`TryComplete` guard. **Complexity: L** (most of this track's real coverage).

## Track B — Badge core (independent of Track A, except Speedster rule)
- [ ] B1. Domain: trim `BadgeCode` to `LoginFirst, CourseDone, Speedster`. `UserBadge`: add nullable `CourseId`. `IBadgeRepository`: `GetByCodeAsync`, `GetAllAsync`, `GetByUserAsync`, `HasBadgeAsync(userId,badgeId,courseId?)`, `TryAddAsync` (bool), `SaveChangesAsync`. **Complexity: S.**
- [ ] B2. Infrastructure: `UserBadgeConfiguration` — add `course_id` + FK to `courses` (Restrict), drop `IX_user_badges_UserId_BadgeId`, add `NULLS NOT DISTINCT` unique index on `(UserId,BadgeId,CourseId)` (Postgres 17, confirmed supported). `BadgeConfiguration`: seed the 3 badge rows (`HasData`, Spanish name/description). `BadgeRepository` impl (23505 catch in `TryAddAsync`). Migration `AddCourseScopeToUserBadgesAndSeedBadges`. **Complexity: M.**
- [ ] B3. Application: `IBadgeAwardService`/`BadgeAwardService` (`OnUserLoggedInAsync`, `OnCourseCompletedAsync`, `OnCourseExamPassedAsync` — this 3rd method's real logic depends on Track A's `Duration`, stub/skip until Integration). Rule interfaces (`ILoginBadgeRule`, `ICourseCompletionBadgeRule`, `IExamPassedBadgeRule`). `FirstLoginRule`, `CourseCompletedRule` (Speedster is Integration, needs Track A). Explicit DI registration (reflection scan only covers handlers/validators). **Complexity: M.**
- [ ] B4. Application: `GetMyBadgesQuery`+handler (earned badges, per-course title where applicable). `BadgesController`: `GET /api/badges/me`. **Complexity: S.**
- [ ] B5. Tests: `BadgeAwardServiceTests` (already-owned → no insert/no notify; race via `TryAddAsync=false` → no notify; exception → logged+swallowed), `FirstLoginRuleTests`, `CourseCompletedRuleTests`, `GetMyBadgesHandlerTests`. **Complexity: M.**

## Track C — Notifications plumbing for badge-earned (small, independent)
- [ ] C1. `INotificationRepository.AddAsync` + implementation (currently missing — the existing Notifications CRUD never needed to create rows from server-side code before). **Complexity: S.**
- [ ] C2. `INotificationPublisher` (Application/Features/Notifications/Services) — stages the in-app row now; email/SignalR channels are no-ops/deferred for this slice (see design decision: no email, no toast contract change yet), built as a reusable seam for the other 2 MVP3 notification triggers later. **Complexity: S.**
- [ ] C3. Tests: `NotificationPublisherTests`. **Complexity: S.**

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

## Progress
(filled in as completed)
