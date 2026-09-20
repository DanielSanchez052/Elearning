# Feature: Unit tests for Quizzes, Notifications, and Reports handlers

## Objective
Close the test-coverage gap flagged as pending in `src/backend/PROYECTO_COMPLETADO_EVALUACIONES.md` (Quizzes scoring logic tests) and confirmed by direct inspection: 16 CQRS handlers across three features have zero unit test coverage.

## Why
- `dotnet test --list-tests` shows 445 tests total, but grouping by feature shows Quizzes has only validator tests (0 handler tests), and Notifications/Reports have none at all.
- Cross-referencing `ELearning.Application/Features/**` handler classes against `ELearning.Tests/Unit/Aplication/Features/**` test classes confirmed the exact gap (see Scope).
- Enrollments (the other module flagged in docs) is already fully covered (4/4 handlers, 21 tests) — out of scope here.

## Scope (16 handlers, 5 work units)
1. **Quizzes admin CRUD handlers** (6): `CreateQuizQuestionHandler`, `UpdateQuizQuestionHandler`, `DeleteQuizQuestionHandler`, `CreateQuizOptionHandler`, `UpdateQuizOptionHandler`, `DeleteQuizOptionHandler`
2. **SubmitQuizHandler** (1): scoring/attempt logic — most complex handler in the app (enrollment checks, required-lesson gating, attempt limits, score calculation, course-completion side effect). Own work unit due to complexity.
3. **Quizzes query handlers** (3): `GetLessonQuizzesHandler`, `GetCourseExamHandler`, `GetUserQuizResultsHandler`
4. **Notifications handlers** (4): `GetMyNotificationsHandler`, `GetUnreadNotificationsCountHandler`, `MarkAllNotificationsAsReadHandler`, `MarkNotificationAsReadHandler`
5. **Reports handlers** (2): `GetDashboardHandler`, `GetLeaderboardHandler`

## Constraints / conventions
- Test project: `src/backend/ELearning.Tests` (xUnit + Moq), mirror folder structure `Unit/Aplication/Features/<Feature>/<Handler>Tests.cs`.
- Pattern reference: `ELearning.Tests/Unit/Aplication/Features/Enrollments/EnrollInCourseHandlerTests.cs` — constructor-injected `Mock<IRepo>`, `Result<T>`/`ResultErrorType` assertions, `Times.Once` verification on save calls.
- No new production code expected; these are characterization/regression tests for already-implemented handlers.

## TDD mode
Resolved: **strict TDD is enabled globally**, but this work backfills tests for already-shipped production code — there is no new behavior to drive with a classic RED step. Applicable practice: write the test asserting the documented/expected contract, run it, and treat the run as verification evidence.
- If a test passes immediately → confirms existing behavior matches contract (recorded as the "GREEN" evidence).
- If a test fails against existing handler code → that is a real bug, not a red herring; it gets flagged and fixed (or reported back) rather than weakened to pass.
Runner: `dotnet test ELearning.Tests/ELearning.Tests.csproj`

## Delivery strategy
`ask-on-risk` (default). Each work unit closes with one commit on a feature branch (`test/quizzes-notifications-reports-coverage`), Conventional Commits, no AI attribution per user's global rule.

## Tasks
- [x] 1. Quizzes admin CRUD handler tests (6 classes) — delegated writer
- [x] 2. SubmitQuizHandler tests (scoring logic) — delegated writer
- [ ] 3. Quizzes query handler tests (3 classes) — delegated writer
- [ ] 4. Notifications handler tests (4 classes) — delegated writer
- [ ] 5. Reports handler tests (2 classes) — delegated writer

## Progress
(updated per task as completed, with commit hash and dotnet test result)

### Task 1 — Quizzes admin CRUD handler tests (done)
- Created 6 test files under `ELearning.Tests/Unit/Aplication/Features/Quizzes/`:
  `CreateQuizQuestionHandlerTests.cs`, `UpdateQuizQuestionHandlerTests.cs`, `DeleteQuizQuestionHandlerTests.cs`,
  `CreateQuizOptionHandlerTests.cs`, `UpdateQuizOptionHandlerTests.cs`, `DeleteQuizOptionHandlerTests.cs`.
- 27 new `[Fact]`/`[Theory]` tests added, covering not-found/validation paths and happy paths (entity construction,
  repository calls, `SaveChangesAsync` verified `Times.Once`).
- `dotnet test ELearning.Tests/ELearning.Tests.csproj --filter "FullyQualifiedName~Quizzes"` →
  `Correctas! - Con error: 0, Superado: 67, Omitido: 0, Total: 67` (27 new + 40 pre-existing validator tests), all green on first run.
- No production bugs found; all 6 handlers behave as documented by their own inline validation.

### Task 2 — SubmitQuizHandler tests (done)
- Created `ELearning.Tests/Unit/Aplication/Features/Quizzes/SubmitQuizHandlerTests.cs` covering: basic
  validation (empty UserId, null/empty SelectedOptionIds, missing LessonId+CourseId), lesson-context and
  course-context not-found/forbidden paths, inactive enrollment, lesson prerequisite gating (missing required
  lesson before target lesson) and final-exam gating (missing any required lesson), no-questions-found,
  answer-count mismatch, attempt-limit logic (first attempt, already-passed, max-attempts-reached, attempts
  remaining/increment), invalid selected option (not found / wrong question), scoring (partial, all-correct,
  and score-exactly-equals-passScore boundary), the `TryComplete` side effect on the final exam
  (pass → enrollment completed; fail → not completed), repository interaction verification
  (`CreateAttemptAsync` once per question, `CreateResultAsync`/`SaveChangesAsync` once each), and full
  `QuizResultDto` field verification.
- 35 new `[Fact]` tests added (used fixture helper methods instead of `[Theory]`/`[InlineData]` — each branch
  needed distinct mock wiring, so parameterizing added more complexity than it removed).
- `dotnet test ELearning.Tests/ELearning.Tests.csproj --filter "FullyQualifiedName~SubmitQuiz"` →
  `Correctas! - Con error: 0, Superado: 35, Omitido: 0, Total: 35, Duración: 238 ms`, all green on first run.
- No production bugs found. Confirmed the score-equals-passScore boundary passes (`score >= passScore`), and
  confirmed `TryComplete` on the final-exam path is unconditionally successful whenever `isPassed` is true —
  because the required-lesson gating check (step before question retrieval) already forbids reaching the
  scoring step unless every required lesson is completed, so `TryComplete`'s `IsSubsetOf` check can never fail
  by the time it's called.
