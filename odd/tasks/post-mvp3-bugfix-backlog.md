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
- [ ] **T1** (R-int-004) `BadgeAwardServiceTests.cs:120-140` — rename `AnyHook_...` tests to name the single hook they actually exercise, or turn into a `Theory` over all 3.
- [ ] **T2** (R-int-005) `badge.types.ts:7` — `UserBadgeDto.description`: `string` → `string | null`, fix any consumer that assumed non-null.
- [ ] **T3** (R-int-006) `SpeedsterRuleTests.cs:33-34` — reflection field-name string literal → `nameof(...)`.
- [ ] **T4** (R-int-001) `IBadgeAwardService.cs:7-11` + 3 call sites — make cancellation actually propagate (catch `Exception` filtered `when (ex is not OperationCanceledException)` at every call site), matching the doc comment instead of rewriting it.

Route: direct inline (4 single-file, already-understood edits) or one bundled writer — whichever keeps this session's context thinnest at execution time.

### Group 2 — real bugfixes, backend (medium complexity)
- [ ] **T5** (R-int-002, most impactful) `BadgeAwardService.cs:~90-110` — isolate each proposal's `PublishAsync` in its own try/catch inside the award loop (log+continue instead of aborting the whole batch), and make sure the final `SaveChangesAsync` for staged notifications always runs even if one proposal's publish failed for another badge. New tests: partial-failure mid-batch still awards+saves the other proposals.
- [ ] **T6** (R-int-003) `LoginCommand.cs:64-72` — wrap the `OnUserLoggedInAsync` call with a linked `CancellationTokenSource` timeout (~2s) so a slow badge store can't add unbounded latency to login. New test: badge call exceeding the timeout doesn't fail login and doesn't block past the bound.
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

## Deferred (not in this pass)
- **R3-001** — exam-session concurrency guard has no automated test; needs a new integration-test project against real Postgres. Own follow-up when that infra is decided.

## Delivery strategy
`ask-on-risk` (session default). One commit per task (T1-T12), Conventional Commits.
Branch: `fix/post-mvp3-bugfix-backlog` from `main`.

## Progress
(filled in as tasks complete)
