# Admin course/quiz creation: undefined id bugs

## Objective
Fix a class of bugs where the admin panel's "create" flows for quiz questions
and courses break because the frontend assumes a `{ value: "<guid>" }` response
envelope, when this codebase's `ToActionResult<T>` (`ControllerExtensions.cs:14`)
actually returns the raw `Result<T>.Value` on success — for a `Result<Guid>`,
that's the bare GUID string, not an object.

## Reported
User: "al intentar crear una pregunta se guarda la pregunta, sin embargo al
tratar de guardar las respuestas al parecer el endpoint se esta construyendo
mal '/api/admin/quizzes/questions/undefined/options' donde saca un error 404".

## Scope
Found by grepping the frontend for every `Result<Guid>`-returning create
endpoint (`RegisterUser`, `CreateCourse`, `EnrollInCourse`, `CreateLesson`,
`CreateQuizOption`, `CreateQuizQuestion`) against how each one's response is
consumed. Exactly 2 read a nonexistent `.value` property; the other 4 are
fire-and-forget on the frontend (nothing reads the resolved id), so they don't
hit this bug despite having the same response shape.

## Tasks
- [ ] **T1** — `CreateQuizQuestion` → `AdminQuizzesPage.tsx:92` (`res.data.value`).
  Already spawned as a separate background task (`task_ddf1d069`, worktree
  `fix/quiz-option-questionid-undefined`) before this doc existed — not
  duplicated here, tracked for cross-reference only.
- [x] **T2** — `CreateCourse` → same shape, `hooks/admin/courses.ts:51`
  (`.then((r) => r.data.value)`) + wrong declared response type in
  `api/admin/courses.ts:71` (`axios.post<{ value: string }>`, should be
  `axios.post<string>`).
- [x] **T3** (found via live verification of T2, same flow) — even with T2
  fixed, `AdminCourseFormPage.tsx:89`'s `useAssignCountries(id ?? '')` binds
  `courseId` at component-mount time from the route param (empty for a
  brand-new course) and never updates to the just-created course's real id —
  so assigning countries to a new course always hit
  `PUT /admin/courses//countries` (empty id) → 404, regardless of T2.
- [x] **T4** (found via the same live verification) — `navigate(`/courses/${courseId}/edit`)`
  pointed at a route that doesn't exist (`/courses/:id` only, no `/edit`
  variant); the real admin edit route is `/admin/courses/:id/edit`. This was
  already broken before T2/T3 (silently masked — `/courses/undefined/edit` was
  *also* a 404, just for the wrong reason), so it surfaced only once T2+T3 made
  the id resolve correctly.

## TDD mode
No frontend test runner in this repo (confirmed by prior sessions). Verified
via live browser repro instead: created a course end-to-end (title,
description, one country, submit) against the real dev API + Postgres,
confirmed `POST /admin/courses` → 200, `PUT /admin/courses/{realGuid}/countries`
→ 204 (was 404 before), and navigation landed on the real
`/admin/courses/{realGuid}/edit` edit page (was a 404 page before). `tsc -b
--noEmit` clean after each change.

## Delivery strategy
Single work unit — all 3 fixes are the same root-cause investigation on the
same user-reported flow, discovered sequentially while verifying one another.
One commit. Branch: `fix/admin-course-quiz-creation-undefined-id` from `main`.

## Progress

### T2 + T3 + T4 — done
Root cause (T2): `ControllerExtensions.cs`'s `ToActionResult<T>` returns
`controller.Ok(result.Value)` — for `CreateCourseHandler` (`Result<Guid>`),
the response body is the raw GUID string. `hooks/admin/courses.ts:51` assumed
a `{ value }` wrapper. Fixed both the hook (`.then((r) => r.data)`) and the
API client's declared type (`axios.post<string>` instead of
`axios.post<{ value: string }>`).

Live-verifying T2 surfaced T3: `AdminCourseFormPage.tsx:89`
`useAssignCountries(id ?? '')` closes over `id` (the route param) at mount —
always empty for `/admin/courses/new`. Even with `courseId` now resolving
correctly from `createCourse.mutateAsync()`, calling
`assignCountries.mutateAsync(...)` still targeted the stale empty id
(`PUT /admin/courses//countries` → 404). Fixed by calling
`coursesApi.assignCountries(courseId, { countryIds })` directly for the
create-new-course path (the existing `assignCountries` mutation hook is still
used, unchanged, for the edit-existing-course path, where `id` really is
correct from the route).

Re-verifying surfaced T4: `navigate(`/courses/${courseId}/edit`)` — no such
route (`App.tsx` only has `/courses/:id` → `CourseDetailPage`, no `/edit`
variant; the real edit route is `/admin/courses/:id/edit`). Fixed the path.

**Live verification** (real dev Postgres + API, admin session):
1. First attempt (before T3/T4 fixes applied to the browser session): course
   created (200), but `PUT .../countries` → **404** (empty id) and the page
   showed "Ocurrió un error inesperado."
2. After the T3 fix: `PUT /admin/courses/77ed03a5-.../countries` → **204**,
   but navigation landed on `/courses/77ed03a5-.../edit` → **404 page** (T4
   not yet fixed).
3. After the T4 fix: full round-trip confirmed — `POST /admin/courses` → 200,
   `PUT /admin/courses/{realGuid}/countries` → 204, navigated to
   `/admin/courses/{realGuid}/edit` → real "Editar curso" page rendered with
   the course's lessons section.

`tsc -b --noEmit`: clean after all 3 fixes.

**Dev-data cleanup**: the 3 verification attempts created 3 real rows in the
local `courses` table (titles "Curso de prueba bugfix..."/"Curso final
verificado") plus their `course_countries` rows. Deleted directly via
asyncpg (the admin UI's own delete-course button didn't fire reliably through
scripted coordinate clicks). Verified only the 2 pre-existing real courses
("Excel Avanzado", "Introducción a la Programación") remain.

**Not fixed (found, out of scope, flagged separately)**: `AdminCoursesPage`'s
"Nuevo curso" link has `href="/courses/new"` — same missing-`/admin`-prefix
mistake as T4's original bug, but on the list page's entry point rather than
the form's post-submit redirect. Not touched here since it wasn't part of the
reported symptom and deserves its own look (worth checking whether it's
genuinely dead/unreachable or if users are hitting a 404 before ever reaching
the form).
