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
- [x] **T7** (R3-003) `StartCourseExamCommand.cs:83-107` + `QuizRepository.TryAddExamSessionAsync` — distinguish which unique index a `23505` violation hit (inspect Postgres constraint name) so a stale-attempt-index collision doesn't return `Conflict` forever; re-derive the next attempt number and retry once instead. Unit-testable by mocking the constraint name.
- [x] **T8** (smoke-test bug) `CreateQuizQuestionCommand.cs:40-43,63-66` — `cmd.LessonId == Guid.Empty` doesn't catch `null`; fix to return a proper 400 validation error instead of throwing → 500.
- [x] **T9** (smoke-test bug) `AdminQuizzesController.CreateQuestion` / `QuizzesController.SubmitCourseExam` — investigate the unbound-request-body → `NullReferenceException` → 500 path (root cause not investigated yet) and add proper model-binding validation.

Route: delegated writer (each touches backend handler + repository/controller + tests — 2+ non-trivial files per task).

### Group 3 — frontend fixes (medium complexity)
- [x] **T10** (R3-002) `QuizSessionPage.tsx:300-311` — on a "ya aprobaste"/"sin intentos" error from `exam/start`, fall back to showing the existing exam-results view instead of a bare error message.
- [x] **T11** (smoke-test bug) `useBadges.ts` — invalidate `badgeKeys.mine()` after lesson-completion and quiz-submission mutations succeed, so a freshly-earned badge doesn't wait out the 60s `staleTime`.

Route: delegated writer (frontend, no test runner — characterize via `tsc -b --noEmit` + eslint + manual check).

### Group 4 — new UI feature (highest complexity)
- [x] **T12** (new bug, notification bell) `NotificationBell.tsx` + new dropdown/panel component — wire `onClick` to open/close, render the notification list from `useNotifications()`, mark-as-read on click via `useMarkNotificationRead()`, close on outside-click/escape. Wire the two backend endpoints that exist but nothing calls yet: `GET /notifications/unread-count` (optional, `useNotifications()` already derives count client-side so this may not be needed) and `PUT /notifications/mark-all-read` (a "marcar todas como leídas" action in the panel).

Route: delegated writer (new component + wiring existing hooks + AppHeader — 2+ non-trivial files, genuine new UI).

## Follow-ups (non-blocking, from all 3 Gentle AI reviews of this branch — all approved)
Consolidated into one list (previously split across 2 sections that drifted out of
sync with each other — that was itself a flagged finding, R2-backlog-doc-self-inconsistent
/ R2-backlog-followup-count-mismatch, fixed by merging into this single section).

**Fixed during correction passes** (kept here for the audit trail):
- [x] **R2-001** — this doc's `Progress` section was out of date vs. its own diff. Fixed.
- [x] **R1-exam-attempt-bump-bypasses-limit-check** / **R3-attempt-retry-bypasses-limit** — T7's attempt-collision retry didn't re-check `maxAttempts` before opening a session past the limit. Fixed: returns `ValidationFailure` when `attemptNumber + 1 > maxAttempts` instead of retrying.
- [x] **R2-publish-oce-filter-inconsistent** / **R3-publish-oce-skips-save** — `BadgeAwardService`'s per-proposal publish catch used a narrower cancellation filter than the other 3 call sites. Aligned.
- [x] **R1-backlog-doc-restates-seed-credentials** — this doc pasted the seeded dev password in 2 places. Redacted to reference `ELearning.API.http` instead.
- [x] **R2-bell-toggle-vs-outside-click** / **R3-bell-toggle-reopens** — clicking the bell to close the panel actually closed-then-immediately-reopened it (the panel's outside-click handler fired before the button's own toggle, since the bell wasn't excluded from the "outside" check). Fixed: lifted a `containerRef` (covering button + panel) from `NotificationBell` into `NotificationPanel`, replacing the panel-only ref. Live-verified in the browser: bell now opens, closes, and outside-click/Escape all work correctly.
- [x] **R3-retry-limit-branch-untested** — the `attemptNumber + 1 > maxAttempts` guard (above) had no test. Added `HandleAsync_AttemptNumberCollisionAtAttemptLimit_ReturnsValidationFailureWithoutRetrying`, real RED→GREEN (RED: `Expected: Validation, Actual: Conflict`).

**Still open (deferred, non-blocking):**
- [ ] **R4-001** / **R4-login-timeout-silent** / **R4-login-badge-timeout-unobserved** — when the 2s login badge timeout trips, nothing is logged (`LoginHandler` has no `ILogger`; would need constructor injection + updating every existing test that constructs it).
- [ ] **R4-002** / **R4-timeout-drops-notification** / **R4-timeout-drops-staged-notifications** — if the login timeout trips between a `TryAddAsync` insert and the trailing `SaveChangesAsync`, the badge is saved but its notification is silently dropped for good (`HasBadgeAsync` blocks re-award). Same root shape as T5/R-int-002, reachable specifically via the timeout path.
- [ ] **R4-003** — the login timeout is hardcoded (not injectable via `TimeProvider`/options), so its test spends ~2s of real wall-clock time per run and the bound can't be tuned without a code change.
- [ ] **R3-timeout-bound-unproved** / **R3-login-timeout-bound-unproved** — `HandleAsync_BadgeServiceTimesOut_LoginStillSucceeds` doesn't actually prove the 2s bound (never asserts elapsed time or that the passed token was cancelled; ties into R4-003).
- [ ] **R3-log-assert-weakened** — the shared `VerifyErrorLogged` test helper matches `It.IsAny<Exception>()`, slightly weaker than the old exact-type match.
- [ ] **R2-003** — T5's partial-failure test picks which fake proposal's `PublishAsync` throws by matching notification title text, coupling it to `BuildNotificationCopy`'s wording.
- [ ] **R4-exam-conflict-unobservable** / **R4-exam-attempt-conflict-unlogged** — neither T7 retry-collision branch (success or still-colliding-after-retry) logs anything; no server-side record for support to investigate.
- [ ] **R4-constraint-drift-silent-fallback** / **R4-constraint-name-fallback-silent** — `QuizRepository`'s constraint-name `switch` silently falls back to `OpenSessionRace` for any unrecognized name (e.g. after a future index rename), quietly reintroducing the exact lockout bug T7 fixed, with no log signal.
- [ ] **R3-constraint-mapping-untested** — no automated test proves the constraint-name → `ExamSessionInsertResult` mapping itself; handler tests mock the enum directly. A small pure-function test (extract the mapping) would close this without needing real Postgres.
- [ ] **R2-unused-open-session-index-const** — `QuizRepository.cs`'s `OpenSessionIndexName` constant is declared but never used in an explicit `switch` arm, only named in a comment; the open-session case is actually just the `default` fallthrough.
- [ ] **R2-null-body-guard-partial** — the null-body guard added in T9 is hand-copied into 2 controller actions; `QuizzesController.SubmitLessonQuiz` still has the identical unguarded `NullReferenceException` path (flagged by T9 as a separate follow-up, `task_02eb50c8`, not yet fixed).

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

Remaining 7 findings recorded as follow-ups below (not fixed now — none are
blocking and several need a bigger change than this correction pass warrants).

### T7 (R3-003) — done
Confirmed the bug: `QuizRepository.TryAddExamSessionAsync` caught `DbUpdateException`
with `ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }`
— matching a `23505` on *either* of the two unique indexes on `exam_sessions`
(`idx_exam_session_user_course_attempt` and `idx_exam_session_one_open_per_user_course`,
both defined in `ExamSessionConfiguration.cs`) — and always returned `false`.
`StartCourseExamHandler.HandleAsync` always treated `false` as "someone else
opened the session concurrently", re-fetching via `GetOpenExamSessionAsync`.
That's correct for the open-session index, but wrong for the attempt-number
index: if a *closed* session already exists at the computed `attemptNumber`
(e.g. a quiz result was reset/deleted, or the stale-session-close branch closed
a session at a different attempt than just computed), `GetOpenExamSessionAsync`
finds nothing (there's no open session — the conflicting row is closed), so the
handler returned a bare `Result.Conflict(...)`. Since `attemptNumber` is
deterministic from `latestResult`, every retry recomputes the exact same
colliding number — a permanent lockout with no self-recovery.

**Fix shape**: changed `IQuizRepository.TryAddExamSessionAsync`'s return type
from `Task<bool>` to `Task<ExamSessionInsertResult>`, a new 3-value enum
(`Inserted` / `OpenSessionRace` / `AttemptNumberCollision`) added to
`ELearning.Domain.Enums`. No existing `Try*Async` method in this codebase
returns anything richer than `bool` (only other match is
`BadgeRepository.TryAddAsync`, also plain `bool`), so this is a new pattern
here — chosen over 2 booleans since the three outcomes are mutually exclusive
(a proper closed enum reads better than `(bool inserted, bool isAttemptCollision)`
at call sites). `QuizRepository` now inspects
`((PostgresException)ex.InnerException).ConstraintName` and matches the two
exact index names from `ExamSessionConfiguration.cs`
(`idx_exam_session_user_course_attempt` → `AttemptNumberCollision`; anything
else, including `idx_exam_session_one_open_per_user_course` or an unrecognized
name from future schema drift, → `OpenSessionRace`, the historically-safe
default). `StartCourseExamHandler` now branches: `OpenSessionRace` keeps the
original re-fetch-and-resume behavior; `AttemptNumberCollision` retries
**exactly once** with `attemptNumber + 1` (re-querying `latestResult` again
would return the identical value here, since nothing changed between the two
calls in the same request — bumping past the colliding number is the only
approach that can actually progress with the read methods this repository
exposes); if that single retry also collides, it's treated as a genuinely
unexpected, non-auto-recoverable state and returns `Result.Conflict(...)`
(documented in code as the intentional non-looping stop condition, not silently
swallowed).

**Testing boundary**: the constraint-name-inspection branch inside
`QuizRepository.TryAddExamSessionAsync` needs a real `PostgresException` thrown
by a live Postgres unique-index violation to exercise honestly — this codebase
has no integration-test project against real Postgres (see R3-001, deferred
above), and constructing a fake `PostgresException` with a `ConstraintName` via
reflection would test Npgsql's plumbing, not this code's logic. That branch is
therefore left to manual/desk verification (read the `switch` against the two
exact index-name strings copied from `ExamSessionConfiguration.cs`), matching
this codebase's existing testing boundary. All real behavioral coverage for
this fix lives at the handler level via `IQuizRepository` mocks in
`StartCourseExamHandlerTests.cs`, which is honest and sufficient here since the
bug and its fix are entirely about how the handler *reacts* to the repository's
signal, not about the Postgres exception handling itself.

RED (bug present — attempt-collision case wired through the new enum but the
handler still collapses it into the old single-branch behavior, to prove the
handler logic itself is the bug, not just a missing enum value):
```
HandleAsync_AttemptNumberCollision_RetriesOnceWithNextAttemptAndSucceeds [FAIL]
  Assert.True() Failure
  Expected: True
  Actual:   False

HandleAsync_AttemptNumberCollisionPersistsAfterRetry_ReturnsConflict [FAIL]
  Moq.MockException: Expected invocation on the mock exactly 2 times, but was 1 times:
  r => r.TryAddExamSessionAsync(It.IsAny<ExamSession>(), CancellationToken)

Con error! - Con error:     2, Superado:    18, Omitido:     0, Total:    20, Duración: 284 ms - ELearning.Tests.dll (net10.0)
```
(A third new test, the open-session-race-on-retry edge case, passed even under
the unfixed code — it happens to take the same re-fetch path as the existing
concurrent-race test, so it's a coincidental pass, not evidence the fix isn't
needed; the two failures above are the real RED.)

GREEN (fix applied, filtered to the handler test class):
```
Correctas! - Con error:     0, Superado:    20, Omitido:     0, Total:    20, Duración: 232 ms - ELearning.Tests.dll (net10.0)
```

Full suite after the fix:
```
Correctas! - Con error:     0, Superado:   638, Omitido:     0, Total:   638, Duración: 2 s - ELearning.Tests.dll (net10.0)
```
(635 pre-existing + 3 new tests, 0 regressions.)

### T8 (smoke-test bug) — done
Confirmed and found the precise trigger: `CreateQuizQuestionValidator` already validates
"exactly one of LessonId/CourseId is present", but not that it's the *right* one for
the declared `Type`. So `Type=PerLesson` with `LessonId=null` but a (mismatched)
`CourseId` provided passes validation and reaches the handler — where
`cmd.LessonId == Guid.Empty` is `false` for `null`, so `cmd.LessonId.Value` throws
`InvalidOperationException` → unhandled → 500. Same shape for `CourseExam`/`CourseId`.
Fix: `cmd.LessonId is null || cmd.LessonId == Guid.Empty` (and the `CourseId`
equivalent) in `CreateQuizQuestionCommand.cs`.

RED (bug reproduced, fix stashed first via `git stash` to get a genuine failure):
```
Con error ...HandleAsync_CourseExam_NullCourseId_ReturnsValidationFailureInsteadOfThrowing
  System.InvalidOperationException : Nullable object must have a value.
Con error ...HandleAsync_PerLesson_NullLessonId_ReturnsValidationFailureInsteadOfThrowing
  System.InvalidOperationException : Nullable object must have a value.
Con error! - Con error:     2, Superado:     5, Omitido:     0, Total:     7
```

GREEN (fix reapplied via `git stash pop`):
```
Correctas! - Con error:     0, Superado:     7, Omitido:     0, Total:     7, Duración: 122 ms
```

Full suite: **640/640** (638 pre-existing + 2 new tests, 0 regressions). Not fixed:
the validator's deeper gap (accepting a mismatched FK for the declared `Type`) —
out of scope for this specific finding, left as-is.

### T9 (smoke-test bug) — done
Root cause investigated and confirmed (previously unknown). `ELearning.API.csproj`
does have `<Nullable>enable</Nullable>` and both controllers have `[ApiController]`,
so the framework's implicit-required-for-non-nullable-`[FromBody]`-parameters
behavior exists in principle — but `Program.cs` explicitly disables the mechanism
that would ever act on it:
```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
```
This flag turns off `[ApiController]`'s automatic "ModelState invalid → 400"
short-circuit *app-wide* (it was set deliberately so this codebase's manual
`Result`/`ToActionResult` validation pattern, e.g. `Result.ValidationFailure`,
is the only thing that produces 400s — not System.Text.Json/ModelState). One
side effect nobody had traced: when `[FromBody]` binding produces a `null`
`request` (literal JSON `null`, an empty body, or non-UTF-8 bytes that make
`System.Text.Json` throw and get recorded as a ModelState error instead of
short-circuiting), the action method still executes with `request == null`,
and both handlers dereference a property on `request` in their first lines
(`request.LessonId` / `request.SelectedOptionIds`) — an unhandled
`NullReferenceException`, which `ExceptionHandlingMiddleware`'s exception-type
switch has no case for, so it falls through to the generic 500.

Verified with a live repro, not just reasoning from docs: ran the API via
`dotnet run --project ELearning.API --launch-profile http` (`elearning-backend`
launch config) against the real dev Postgres, logged in with the seeded
student/admin accounts (credentials in `ELearning.API.http`, not repeated here),
then hit both endpoints with `curl`:
- `POST /api/quizzes/courses/{id}/exam/submit` with non-UTF-8 bytes
  (`--data-binary` of raw `\xFF\xFE\x00\x01...`), literal `null`, and an empty
  body — all three: real `500`, log shows
  `System.NullReferenceException ... at QuizzesController.SubmitCourseExam(...)
  line 76` (the `request.SelectedOptionIds` line).
- `POST /api/admin/quizzes/questions` with literal `null` — same real `500`.

Fix (framework-idiomatic given the constraint that `SuppressModelStateInvalidFilter`
can't be relied on and is staying — it's load-bearing for the rest of the app's
validation pattern): an explicit `if (request is null) return
this.ToActionResult(Result.ValidationFailure<T>("El cuerpo de la solicitud es
requerido"));` at the top of each action, matching this codebase's existing
`Result` → `ToActionResult` → `BadRequest` shape (same `{ error: "..." }` body
other validation failures already return) instead of inventing a new response
shape or a global exception-handling change.

Re-verified live after the fix, same three requests: all now return real `400`
`{"error":"El cuerpo de la solicitud es requerido"}` instead of `500`. A
sanity-check well-formed body to `CreateQuestion` still reaches the handler
and returns its normal validation result (`FK: Debe proporcionar LessonId o
CourseId`), confirming the null-guard doesn't shadow real requests.

No controller-level test infrastructure exists in this codebase (`ELearning.Tests`
has no `WebApplicationFactory`/`TestServer`/direct-controller-instantiation
tests anywhere — same gap `mvp3-badges.md` A4 already noted: "no controller test
project; route smoke-checked"). Did not force new test-infrastructure into this
one small fix; verified via the live repro above instead (before/after, both
against a real running server and real Postgres).

Full suite: **640/640**, 0 regressions (no handler/unit-level code changed —
the fix is controller-only, so the existing test count is unaffected).

Not fixed (out of scope for T9, flagged separately): `QuizzesController.SubmitLessonQuiz`
(`POST /api/quizzes/lessons/{id}/submit`) has the identical `request.SelectedOptionIds`
dereference and the same null-body 500 risk, but it wasn't named in this task's
scope.

### Gentle AI review (Group 1 + T5 + T7 + T8, lineage `review-b86ea5eb6e085f5e`) — approved, 2 findings fixed immediately
17 files, 722 lines, high tier (auth hot path). 11 non-blocking findings across
4 lenses. Two were fixed right away since they were direct, cheap corrections:

- **R1-exam-attempt-bump-bypasses-limit-check** / **R3-attempt-retry-bypasses-limit**
  (same issue, risk + reliability lenses): T7's retry-once logic opened a session
  at `attemptNumber + 1` without re-checking `maxAttempts`. If the colliding
  attempt was already the last allowed one, the retry could open a session past
  the course's attempt limit. Fixed in `StartCourseExamCommand.cs`: return the
  same `ValidationFailure` the earlier attempt-limit check uses when
  `attemptNumber + 1 > maxAttempts`, instead of retrying past it.
- **R2-publish-oce-filter-inconsistent** / **R3-publish-oce-skips-save** (same
  issue, readability + reliability lenses): `BadgeAwardService`'s new
  per-proposal `PublishAsync` catch used `ex is not OperationCanceledException`
  without the `|| !ct.IsCancellationRequested` guard added to the other 3
  call sites — so a stray `OperationCanceledException` from the notification
  publisher, not tied to `ct`, would still abort the batch loop (skipping later
  proposals and the trailing `SaveChangesAsync`). Aligned the filter to match.

Also fixed the doc self-inconsistency the readability lens flagged
(**R2-backlog-doc-self-inconsistent**): this file said "8 findings" for a list
of 7, and R2-001 was still unchecked despite its own text saying it was fixed —
both corrected. Also tightened the `ExamSessionInsertResult.AttemptNumberCollision`
XML doc (**R2-enum-doc-contradicts-handler**) to describe the real bounded-retry
contract instead of a vaguer "re-derive and retry" that didn't match the handler.

Remaining 9 findings recorded as follow-ups above (mostly already-tracked
timeout/observability gaps reaffirmed by this review, plus one new: constraint-name
drift silently falling back to the safe default with no log signal).

Full suite after these fixes: **640/640**, 0 regressions (no new tests needed —
these were corrections to already-tested code paths; existing
`StartCourseExamHandlerTests` and `BadgeAwardServiceTests` still cover the
changed branches' outer behavior).

### T10 (R3-002) — done
Confirmed the regression: `QuizSessionPage.tsx`'s course-exam-start-on-load
`useEffect` (line ~98) calls `startCourseExamAsync(courseId)`, and
`applyStartResult`'s reject handler (line ~85-88) sets `startError` to the
backend's message via `getErrorMessage`. Since `StartCourseExamCommand`
returns a `ValidationFailure` with "Ya aprobaste esta evaluación..." or
"Alcanzaste el máximo de N intentos..." for a student who already passed or
exhausted attempts, those cases hit the exact same `startError` branch as a
genuine network/server error — the bare-error block at line ~300-311 (message
+ "Volver al curso" link) — with no way back to see past results.

There was no existing "past attempts" rendering to reuse: the only results
view in this component is the `result ? (...) : (...)` block (line ~353),
which only ever holds a *freshly-submitted* `QuizResultDto` (set by
`submitQuiz`'s `setResult(response.data)`) — never populated from
`examResultsQuery.data`/`attempts` (a `QuizAttemptDto[]`, with just
`attemptNumber`/`score`/`isPassed`/`completedAt`, a lighter shape than
`QuizResultDto`). So a minimal new attempts-list rendering was built inline
in the `startError` branch rather than extracted/reused from elsewhere.

Fix: split the `!isLessonQuiz && !examSession && startError` branch in two.
When `attempts.length === 0` (reusing the existing `attempts` variable, which
already reads `examResultsQuery.data ?? []`), keep the original bare-error
block unchanged — the genuine dead-end case with nothing to show. When
`attempts.length > 0`, render the `startError` message alongside a simple
sorted (`attemptNumber` desc) list of past attempts (attempt number, formatted
date, score, pass/fail), so a student who already passed or ran out of
attempts sees their history instead of a dead end. The `isLessonQuiz` path
(and every other branch) is untouched.

Characterization (no frontend test runner in this repo, matching this
backlog's established convention): `npx tsc -b --noEmit` from
`src/frontend/elearning-web` — clean, no errors. `npx eslint
src/pages/quiz/QuizSessionPage.tsx` reports 4 pre-existing problems (2
`react-hooks/set-state-in-effect` errors at lines 138/274, 1
`@typescript-eslint/no-explicit-any` at line 253, 1 `exhaustive-deps` warning
at line 275) — confirmed pre-existing by running eslint against the
unmodified file via `git stash`/`git stash pop`; the new code introduces no
new lint errors. Manually traced the JSX logic: `attempts` is computed before
the early-return blocks (line ~110-112) so it's available in scope; the new
branch only activates for the course-exam path (`!isLessonQuiz`) with no
active session and a start error, exactly mirroring the original guard.

### T11 (smoke-test bug) — done
Confirmed the bug: `useBadges.ts`'s `useMyBadges` query has `staleTime: 1000 *
60` and `badgeKeys.mine()` was never invalidated by anything — badges are
only ever awarded server-side as a side effect of login, lesson completion,
or quiz/exam submission, and none of those three mutation hooks touched the
badges cache. A badge earned by finishing a lesson or passing a quiz/exam
seconds earlier could still read stale (pre-award) data on the Profile page
for up to 60s.

Fix: added `queryClient.invalidateQueries({ queryKey: badgeKeys.mine() })` to
the existing `onSuccess` callbacks of:
- `useMarkLessonComplete` in `useEnrollments.ts` (alongside its existing
  `enrollmentKeys.mine()` / `enrollmentKeys.progress()` /
  `quizzesKeys.courseExam()` / `quizzesKeys.lesson()` invalidations),
- `useSubmitLessonQuiz` in `quizzes.ts` (alongside `quizzesKeys.lesson()` /
  `quizzesKeys.results.lesson()`),
- `useSubmitCourseExam` in `quizzes.ts` (alongside `quizzesKeys.courseExam()`
  / `quizzesKeys.results.courseExam()`).

Login is deliberately not touched — it's not a TanStack Query mutation with a
query-invalidation hook in this codebase, so there's nothing to add the
badges invalidation to there; a post-login badge award is picked up by the
existing `staleTime`/refetch behavior like any other query. No other mutation
in the app was touched, matching the task's scope (badges are only earned via
these three paths).

Both `useBadges.ts` and the two edited files already used `@/...` absolute
imports for their other imports, so `badgeKeys` is imported the same way
(`import { badgeKeys } from '@/hooks/useBadges'`) in both `useEnrollments.ts`
and `quizzes.ts`.

Characterization (no frontend test runner in this repo): `npx tsc -b
--noEmit` from `src/frontend/elearning-web` — clean, no errors. `npx eslint
src/hooks/useEnrollments.ts src/hooks/quizzes.ts src/hooks/useBadges.ts` —
clean, no errors or warnings on any of the three files. Manually verified no
import cycle: `useBadges.ts` imports only `@tanstack/react-query` and
`@/api/badges`, so `useEnrollments.ts`/`quizzes.ts` importing from it doesn't
loop back.

### T12 (new bug, notification bell) — done
Confirmed the bug: `NotificationBell.tsx`'s `<button>` had no `onClick` at
all, and there was no dropdown/panel component anywhere in the codebase, even
though the data layer (`useNotifications`, `useMarkNotificationRead`,
`notificationsApi`) already existed and was fully unused. Clicking the bell
did nothing.

Component split: kept `NotificationBell.tsx` small (icon button + badge +
open/close `useState`) and added a new sibling
`src/components/layout/NotificationPanel.tsx` for the dropdown itself, mounted
inside the existing `relative` wrapper div so it can be absolutely positioned
under the bell (`AppHeader.tsx` needed zero changes — `<NotificationBell />`
still takes no props). The panel:
- closes on outside-click (`mousedown` + ref check) and on `Escape`
  (`keydown`), both via plain `useEffect`/`document.addEventListener` — no new
  dependency;
- renders the list from `useNotifications()` with a loading state
  ("Cargando notificaciones..."), an empty state ("No tienes notificaciones",
  matching the "No hay certificados aún" tone from `Profile.tsx`), an indigo
  dot + tinted background for unread items (existing dark zinc/indigo
  palette: `bg-[#111118]`, `border-white/[0.08]`, `text-zinc-400`,
  `bg-indigo-500/[0.06]`, `bg-indigo-400` dot), and a relative date formatter
  (`Hace N min` / `Hace N h` / `Hace N d`, falling back to
  `toLocaleDateString()` past a week — no date library in this repo, matches
  the plain `toLocaleDateString()` precedent in `Profile.tsx`);
- has a `max-h-96 overflow-y-auto` list so a long notification list scrolls
  instead of growing the panel unbounded;
- clicking an unread notification calls `useMarkNotificationRead()`.

Data-layer fix required by the task: `useMarkNotificationRead` did not
invalidate the `['notifications']` query on success, so a mark-as-read would
never update the badge/list until the next 30s poll. Added
`queryClient.invalidateQueries({ queryKey: ['notifications'] })` in its
`onSuccess`, matching the `useEnrollments.ts`/`quizzes.ts` pattern.

Mark-all-read: wired it. The backend already exposes a real, tested
`PUT /notifications/mark-all-read` endpoint that nothing called, so added
`markAllNotificationsRead()` to `api/notifications.ts` and a new
`useMarkAllNotificationsRead()` hook next to `useMarkNotificationRead` in
`useNotifications.ts` (same `useMutation` + invalidate-on-success pattern). It
surfaces as a "Marcar todas como leídas" link in the panel header, shown only
when at least one notification is unread. Did *not* wire
`GET /notifications/unread-count` — the client-side derivation already used
in `NotificationBell.tsx` (`notifications.filter(n => !n.isRead).length`) is
correct and sufficient since `useNotifications()` is already polled every
30s; adding a second endpoint call would just be an extra request returning
data the client already has.

Left untouched, out of scope per task: the dead `createNotification` in
`api/notifications.ts` (pre-existing `any`-typed export, unused), and the
orphaned SignalR hub (`NotificationHub.cs` / `Program.cs` `MapHub`) — this is
a plain REST + 30s-polling dropdown, no real-time wiring, no new
`@microsoft/signalr` dependency added.

Characterization (no frontend test runner in this repo): `npx tsc -b
--noEmit` from `src/frontend/elearning-web` — clean, no errors. `npx eslint`
on the three touched/new files (`NotificationBell.tsx`, `NotificationPanel.tsx`,
`useNotifications.ts`) — clean, no errors or warnings. `npx eslint` on
`api/notifications.ts` reports one pre-existing `@typescript-eslint/no-explicit-any`
on the untouched `createNotification: (data: any) =>` line (confirmed via
`git diff` that only the new `markAllNotificationsRead` line was added to
that file); not introduced by this change and left as-is per the task's
explicit scope note.

Visually verified live, not just statically: started both launch configs
(`elearning-backend` on :5277, `elearning-frontend` on :5173) in the Browser
pane, logged in with the seeded student account (credentials in
`ELearning.API.http`, not repeated here), and drove the actual dashboard. Confirmed: the bell
showed a real unread badge (1) from a seeded `BadgeEarned` notification;
clicking it opened the panel with the notification, its message, a correct
"Hace 15 min" relative timestamp, an unread dot, and the "Marcar todas como
leídas" action; clicking the notification called mark-read and the panel/bell
updated live (badge disappeared, unread dot and tint cleared, mark-all action
hid itself) purely from the query-cache invalidation, no manual refresh;
outside-click closed the panel; re-opening and pressing Escape closed it too.
No console errors during any of this. Both dev servers were stopped
afterward.

### Final Gentle AI review (all 12 tasks, lineage `review-7380b18361a8e6ff`) — approved, 5 findings fixed immediately
26 files, 1218 lines, high tier. 13 non-blocking findings across 4 lenses. Five
were fixed right away (see the consolidated Follow-ups section above for the
full list, now merged into one section since the doc having two separate,
drifting Follow-ups sections was itself one of the 13 findings):

1. **The real bug**: clicking the notification bell to close the panel actually
   closed it and immediately reopened it — the panel's document-level
   `mousedown` outside-click handler ran before the button's own `onClick`
   toggle, and the bell wasn't excluded from the "outside" check. Fixed by
   lifting a shared `containerRef` (wrapping both the button and the panel)
   from `NotificationBell` into `NotificationPanel`, replacing the panel's
   previously-local, panel-only ref. Live-verified in the browser: bell open
   → bell click closes it (no reopen) → reopen → outside-click closes it →
   reopen → Escape closes it. All correct now.
2. The attempt-limit guard added in the previous correction pass had no test
   proving it — added one, with real RED (`Expected: Validation, Actual:
   Conflict`) against the guard temporarily removed, then GREEN with it
   restored.
3. Redacted the seeded dev password that 2 Progress entries had pasted in
   plaintext (referenced `ELearning.API.http` instead).
4. Fixed the doc's own self-inconsistent follow-up counts by merging the two
   drifting "Follow-ups" sections into one.

Full suite after these fixes: **641/641** (640 + 1 new test), 0 regressions.
`tsc -b --noEmit` clean on the frontend fix. Remaining 11 findings recorded
in the consolidated Follow-ups section above (mostly timeout/observability
gaps already tracked, plus 2 new minor readability items: an unused constant
in `QuizRepository.cs`, and the duplicated null-body-guard pattern with
`SubmitLessonQuiz` still unfixed).
