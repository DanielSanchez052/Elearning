# Post-MVP3 bugfix backlog

## Objective
Clear the backlog of non-blocking findings and bugs accumulated during MVP3 badges
(Gentle AI review follow-ups + smoke-test bugs) plus the notification-bell dropdown
gap, before starting Nivel Móvil. Source docs: `odd/tasks/mvp3-badges.md`
("Follow-ups" sections) and `odd/tasks/notifications-backend-implementation.md`
("Follow-up bug found later").

## Scope decisions made before starting (flagged to user)
- **R3-001 deferred, out of this pass.** Needs a brand-new integration-test project
  (real Postgres, no such project exists yet) — that's test-infrastructure scope,
  not a bugfix. Left as its own follow-up.
- **R-int-003 fix shape**: bound the login-hot-path badge evaluation with a short
  timeout (~2s) linked to the request's `CancellationToken`, not fire-and-forget
  (a scoped `DbContext` would be disposed before a detached background task
  finished — fire-and-forget is unsafe here without a new DI scope, which is out
  of scope for this fix).

## TDD mode
Strict (user's global default). All 13 items touch already-shipped code — most are
straightforward bugfixes with real RED→GREEN (the bug IS the RED). Exceptions noted
per task where there's no frontend test runner (characterization by manual/tsc check
only, matching prior sessions' convention).

## Tasks

### Group 1 — mechanical / trivial (single file each, low complexity)
- [x] **T1** (R-int-004) `BadgeAwardServiceTests.cs:120-140` — rename `AnyHook_...` tests to name the single hook they actually exercise, or turn into a `Theory` over all 3.
- [x] **T2** (R-int-005) `badge.types.ts:7` — `UserBadgeDto.description`: `string` → `string | null`, fix any consumer that assumed non-null.
- [x] **T3** (R-int-006) `SpeedsterRuleTests.cs:33-34` — reflection field-name string literal → `nameof(...)`.
- [x] **T4** (R-int-001) `IBadgeAwardService.cs:7-11` + 3 call sites — make cancellation actually propagate (catch `Exception` filtered `when (ex is not OperationCanceledException)` at every call site), matching the doc comment instead of rewriting it.

Route: direct inline (4 single-file, already-understood edits) or one bundled writer — whichever keeps this session's context thinnest at execution time.

### Group 2 — real bugfixes, backend (medium complexity)
- [x] **T5** (R-int-002, most impactful) `BadgeAwardService.cs:~90-110` — isolate each proposal's `PublishAsync` in its own try/catch inside the award loop (log+continue instead of aborting the whole batch), and make sure the final `SaveChangesAsync` for staged notifications always runs even if one proposal's publish failed for another badge. New tests: partial-failure mid-batch still awards+saves the other proposals.
- [x] **T6** (R-int-003) `LoginCommand.cs:64-72` — wrap the `OnUserLoggedInAsync` call with a linked `CancellationTokenSource` timeout (~2s) so a slow badge store can't add unbounded latency to login. New test: badge call exceeding the timeout doesn't fail login and doesn't block past the bound.
- [ ] **T7** (R3-003) `StartCourseExamCommand.cs:83-107` + `QuizRepository.TryAddExamSessionAsync` — distinguish which unique index a `23505` violation hit (inspect Postgres constraint name) so a stale-attempt-index collision doesn't return `Conflict` forever; re-derive the next attempt number and retry once instead. Unit-testable by mocking the constraint name.
- [ ] **T8** (smoke-test bug) `CreateQuizQuestionCommand.cs:40-43,63-66` — `cmd.LessonId == Guid.Empty` doesn't catch `null`; fix to return a proper 400 validation error instead of throwing → 500.
- [ ] **T9** (smoke-test bug) `AdminQuizzesController.CreateQuestion` / `QuizzesController.SubmitCourseExam` — investigate the unbound-request-body → `NullReferenceException` → 500 path (root cause not investigated yet) and add proper model-binding validation.

Route: delegated writer (each touches backend handler + repository/controller + tests — 2+ non-trivial files per task).

### Group 3 — frontend fixes (medium complexity)
- [ ] **T10** (R3-002) `QuizSessionPage.tsx:300-311` — on a "ya aprobaste"/"sin intentos" error from `exam/start`, fall back to showing the existing exam-results view instead of a bare error message.
- [ ] **T11** (smoke-test bug) `useBadges.ts` — invalidate `badgeKeys.mine()` after lesson-completion and quiz-submission mutations succeed, so a freshly-earned badge doesn't wait out the 60s `staleTime`.

Route: delegated writer (frontend, no test runner — characterize via `tsc -b --noEmit` + eslint + manual check).

### Group 4 — new UI feature (highest complexity)
- [ ] **T12** (new bug, notification bell) `NotificationBell.tsx` + new dropdown/panel component — wire `onClick` to open/close, render the notification list from `useNotifications()`, mark-as-read on click via `useMarkNotificationRead()`, close on outside-click/escape. Wire the two backend endpoints that exist but nothing calls yet: `GET /notifications/unread-count` (optional, `useNotifications()` already derives count client-side so this may not be needed) and `PUT /notifications/mark-all-read` (a "marcar todas como leídas" action in the panel).

Route: delegated writer (new component + wiring existing hooks + AppHeader — 2+ non-trivial files, genuine new UI).

## Follow-ups (non-blocking, from Gentle AI review of Group 1 + T5, approved)
- [ ] **R4-001** — `LoginCommand.cs`: when the 2s badge timeout trips, nothing is logged. `LoginHandler` has no `ILogger` today (would need constructor injection + updating every existing test that constructs it), so a slow badge store silently and permanently stops awarding login badges with no operator signal. Worth fixing together with a broader look at whether `LoginHandler` should get a logger at all.
- [ ] **R4-002** — `LoginCommand.cs` + `BadgeAwardService.cs`: if the login timeout trips *between* a `TryAddAsync` insert and the trailing `SaveChangesAsync` for its notification, the badge is saved but the notification is silently dropped (same root shape as R-int-002/T5, but reachable via the new timeout path specifically). Same fix family as T5, scoped to the timeout case.
- [ ] **R4-003** — `LoginHandlerTests.cs`: the timeout is hardcoded (not injectable via `TimeProvider`/options), so the new timeout test spends ~2s of real wall-clock time per run and the bound can't be tuned without a code change.
- [ ] **R3-timeout-bound-unproved** — `LoginHandlerTests.cs:191-211`: `HandleAsync_BadgeServiceTimesOut_LoginStillSucceeds` doesn't actually prove the 2s bound — the fake badge call would return the same result whether or not the timeout exists, since the test never asserts elapsed time or that the passed token was cancelled. Would need `TimeProvider` injection (ties into R4-003) or an elapsed-time assertion to be a real RED→GREEN proof.
- [ ] **R3-log-assert-weakened** — `BadgeAwardServiceTests.cs`: the shared `VerifyErrorLogged` helper matches `It.IsAny<Exception>()`, slightly weaker than the old test's exact `InvalidOperationException` match. Low priority (the tests still assert the log fires the right number of times).
- [ ] **R2-001** — this doc, `Progress` section: was out of date when Gentle AI reviewed the Group-1+T5 candidate (T1-T4/T6 were unchecked with no notes even though they were in the same diff). Fixed as part of this same correction pass.
- [ ] **R2-003** — `BadgeAwardServiceTests.cs` (T5's new partial-failure test): picks which of the 2 fake proposals' `PublishAsync` throws by matching the notification title text against `BuildNotificationCopy`'s wording, coupling the test to copy text that isn't shown in the test itself. Picking by `referenceId` or call order would be clearer and copy-change-proof.

## Deferred (not in this pass)
- **R3-001** — exam-session concurrency guard has no automated test; needs a new integration-test project against real Postgres. Own follow-up when that infra is decided.

## Delivery strategy
`ask-on-risk` (session default). One commit per task (T1-T12), Conventional Commits.
Branch: `fix/post-mvp3-bugfix-backlog` from `main`.

## Progress

### T5 (R-int-002) — done
Confirmed the bug: inside `AwardAsync`'s `foreach` loop, `PublishAsync` had no
try/catch, so a notification failure on proposal N unwound past the loop into
`AwardBestEffortAsync`'s outer catch, which logs and returns an empty list —
losing the count of proposals 1..N-1 that were already inserted, skipping
proposals N+1..end entirely for that trigger, and never reaching the final
`SaveChangesAsync`. Fix: record `awarded.Add(...)` right after `TryAddAsync`
succeeds (so the badge counts as awarded regardless of what happens next),
then wrap only the `PublishAsync` call in its own try/catch (log+continue,
same `ex is not OperationCanceledException` filter as the outer wrapper) so
one proposal's notification failure can't lose earlier awards, skip later
proposals, or block the trailing `SaveChangesAsync`.

Since every real rule (`FirstLoginRule`, `CourseCompletedRule`,
`SpeedsterRule`) is the sole registered rule for its trigger and each proposes
at most 1 badge, no genuine single-trigger call in this codebase currently
produces 2+ proposals in one batch. The new test therefore uses two local
fake `ILoginBadgeRule` doubles constructed directly in the test (not real
product rules) so one `OnUserLoggedInAsync` call evaluates 2 proposals in the
same `AwardAsync` batch — the only way to exercise the isolation honestly.

RED (bug present):
```
Assert.Equal() Failure: Values differ
Expected: 2
Actual:   0
Con error! - Con error:     1, Superado:     0, Omitido:     0, Total:     1
```

GREEN (fix applied, filtered to the test class):
```
Correctas! - Con error:     0, Superado:    22, Omitido:     0, Total:    22, Duración: 215 ms - ELearning.Tests.dll (net10.0)
```

Full suite after the fix:
```
Correctas! - Con error:     0, Superado:   635, Omitido:     0, Total:   635, Duración: 2 s - ELearning.Tests.dll (net10.0)
```
(634 pre-existing + 1 new test, 0 regressions.)

### T1-T4, T6 — done (Group 1, done together, 5 commits)
T1: `AnyHook_*` (2 mislabeled tests) expanded into 6 explicit tests, 2 per trigger
(login/course-completion/exam-passed), each covering "repository throws" and
"cancelled". T2: `UserBadgeDto.description` → `string | null` (`Profile.tsx`
already guarded it with a truthiness check). T3: `nameof(UserQuizResult.CompletedAt)`
instead of a string literal for the reflection `SetPrivate` call. T4: all 3
badge-award call sites (`LoginCommand.cs`, `MarkLessonCompleteCommand.cs`,
`SubmitQuizCommand.cs`) now let real cancellation propagate instead of a bare
`catch (Exception)`. T6: `LoginHandler` bounds the badge check with a 2s timeout
(`LoginBadgeTimeout` constant) linked to the request `ct`, so only a genuine
client cancellation propagates — a pure timeout trip is swallowed like any
other best-effort failure.

Encoding note: `LoginCommand.cs` is Windows-1252, not UTF-8 (`contraseña`/`sesión`
are single-byte `ñ`/`ó`). A first pass with the normal edit tool silently turned
both into U+FFFD; caught it, restored the file from `main`, and reapplied both
T4 and T6 there via a raw byte-level replacement instead.

Full suite after Group 1: **634/634**, 0 regressions. `tsc -b --noEmit` clean on
the frontend type change.

### Gentle AI review (Group 1 + T5, lineage `review-f9d0a82f668a4016`) — approved, 2 findings fixed immediately
10 non-blocking findings across all 4 lenses; risk found nothing. Two reliability
WARNINGs were fixed right away since they were cheap and directly caused by this
same change: **R3-oce-filter-inconsistent** / **R3-oce-filter-quiz** — `MarkLessonCompleteCommand.cs`
and `SubmitQuizCommand.cs`'s cancellation filters didn't check `ct.IsCancellationRequested`
like `LoginCommand.cs`'s does, so any stray `OperationCanceledException` from
elsewhere (not tied to the request) would now escape and fail an already-persisted
mutation. Fixed by adding the same `|| !ct.IsCancellationRequested` guard to both
(the quiz one needed `TryAwardBadgesAsync` to take `ct` as a parameter). Also
fixed **R2-002** (readability): the 2s login timeout is now a named
`LoginBadgeTimeout` constant instead of an inline magic number.

Remaining 8 findings recorded as follow-ups below (not fixed now — none are
blocking and several need a bigger change than this correction pass warrants).
