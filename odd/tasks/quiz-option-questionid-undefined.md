# Fix undefined questionId in admin quiz-option creation

## Objective
Fix: in the admin quiz-question composer, creating a question saves it
correctly, but the immediate follow-up call to create its options fails with
a 404, because the request URL is built as
`/api/admin/quizzes/questions/undefined/options`.

## Root cause
`ControllerExtensions.cs`'s `ToActionResult<T>` returns `controller.Ok(result.Value)`
on success — for `CreateQuizQuestionHandler` (`Result<Guid>`), the response
body is the raw Guid string (e.g. `"e9a1a6bf-67f0-437f-9a64-d9ed0e0ebcb1"`),
not `{ value: "..." }`. `AdminQuizzesPage.tsx:92` read `res.data.value`, which
is `undefined` on a raw string.

This is the sibling of the same bug already fixed for `CreateCourse` on
`fix/admin-course-quiz-creation-undefined-id` (branched separately from this
worktree so the two fixes don't collide).

## Fix
- `src/frontend/elearning-web/src/api/admin/quizzes.ts`: `createQuestion` and
  `createOption` both declared `axios.post<{ value: string }>(...)` — changed
  both to `axios.post<string>(...)` to match the real response shape.
- `src/frontend/elearning-web/src/pages/admin/AdminQuizzesPage.tsx:92`:
  `const questionId = res.data.value;` → `const questionId = res.data;`.

No other `.mutateAsync` call in this file captures a resolved id the same
wrong way — `createOption`'s own resolved value isn't read anywhere (its 3
calls in the options loop are fire-and-forget), so no second-order bug like
`CreateCourse`'s `useAssignCountries` one applies here.

## TDD mode
No frontend test runner in this repo. Verified via live browser repro
instead, against the real dev API + Postgres.

## Progress

### Done
`tsc -b --noEmit` clean.

**Live verification** (admin session, course "Excel Avanzado",
`f365429b-5400-43ed-9173-0a7ab4b467bb`):
- Before the fix (reproduced the reported bug first): `POST
  /admin/quizzes/questions` → 200, question saved; `POST
  /admin/quizzes/questions/undefined/options` → **404** for the first option
  (question saved with 0 options, an orphaned row).
- After the fix: created a new question end-to-end — `POST
  /admin/quizzes/questions` → 200 with the question saved, then all 3 `POST
  /admin/quizzes/questions/{realGuid}/options` → **200 OK** each. Confirmed
  in the UI: the new question shows "Opciones: 3".

**Dev-data cleanup**: deleted the 2 test questions created during
verification (one orphaned 0-option question from the pre-fix repro, one
full 3-option question from the post-fix verification) directly via asyncpg —
the admin UI's own delete button didn't fire reliably through scripted
coordinate clicks (same friction hit on the `fix/admin-course-quiz-creation-undefined-id`
branch's course cleanup). Confirmed only the original 2 real exam questions
remain for this course.

**Note on how this was verified**: the Browser-pane preview tool always runs
named launch configs against the main checkout's working directory, not this
worktree's path — so live verification was done by applying this same diff as
a temporary patch to the main checkout, testing it there, then reverting that
temporary patch once confirmed. The commit here is this worktree's own,
independent of that temporary test copy.

### Gentle AI review (lineage `review-59033fbaedd2b028`) — approved, 1 finding fixed
Medium tier (1 lens, review-reliability), 3 files, 71 lines. One non-blocking
WARNING: `questionId` (from `res.data`) was used without a runtime check —
if the response shape ever regresses again, the same undefined-id 404 would
silently reappear with no automated test to catch it. Fixed immediately since
it was cheap: added `if (!questionId) throw new Error(...)` right after
reading it, so a future regression fails loudly instead of 404ing silently
per option. `tsc -b --noEmit` clean after the addition.
