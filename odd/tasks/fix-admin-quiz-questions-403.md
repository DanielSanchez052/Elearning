# Bugfix: Admin quiz-question management returns 403 Forbidden

## Objective
`AdminQuizzesPage.tsx` ("Gestión de Exámenes"/"Gestión de Preguntas") silently shows "0 preguntas" for any admin/instructor who isn't personally enrolled as a student in the course — found live via browser navigation testing (`GET /api/quizzes/courses/{id}/exam` → 403, swallowed by the UI as an empty list). This breaks the content-authoring workflow for the exact users who need it (MVP2 2.3).

## Root cause
`AdminQuizzesPage.tsx:11,52-53` imports and calls `useLessonQuizzes`/`useCourseExam` from `src/hooks/quizzes.ts` — the STUDENT-facing hooks, which call `GetLessonQuizzesHandler`/`GetCourseExamHandler`. Both handlers gate on the caller being enrolled, with an active enrollment, having completed prerequisite lessons (see `ELearning.Application/Features/Quizzes/Queries/GetLessonQuizzesQuery.cs` and `GetCourseExamQuery.cs`). An admin/instructor managing course content has none of that, so every call returns `Result.Forbidden`.

There is no admin-side listing endpoint at all: `AdminQuizzesController.cs` (`/api/admin/quizzes`) only has `POST`/`PUT`/`DELETE` for questions/options — zero `GET` actions.

## Bonus finding while fixing this
The existing student-facing `QuizOptionDto` intentionally omits `IsCorrect` (anti-cheat — verified in an earlier test pass with a reflection assertion that this field doesn't exist on the DTO). But `AdminQuizzesPage.tsx:175,185` and `QuestionComposerModal.tsx` already read `option.isCorrect` when populating the edit form for an EXISTING question — meaning even once the 403 is fixed, if the admin listing reuses the student DTO, every option would show as unchecked/incorrect when editing, silently losing which answer was actually marked correct. `src/types/quiz.types.ts:26` even has a comment acknowledging this: `isCorrect?: boolean; // No viene en respuestas GET (seguridad)`. Fix this in the same pass by giving the admin listing its own DTO that DOES include `IsCorrect` — do NOT add `IsCorrect` to the shared student `QuizOptionDto`, that would reintroduce the anti-cheat leak for students.

## Scope

### Backend
1. New DTOs (admin-only — do not modify `QuizQuestionDto`/`QuizOptionDto`, those stay student-safe): add `QuizQuestionAdminDto`/`QuizOptionAdminDto` in `ELearning.Application/Features/Quizzes/DTOs/` (new file, e.g. `QuizAdminDtos.cs`), same shape as the existing DTOs but `QuizOptionAdminDto` includes `IsCorrect`.
2. New queries (no enrollment gating — this is role-protected at the controller level via `[Authorize(Roles = "admin,superadmin,instructor")]`, matching `AdminQuizzesController`'s existing attribute):
   - `GetLessonQuizQuestionsAdminQuery(Guid LessonId) : IQuery<IReadOnlyList<QuizQuestionAdminDto>>` → handler calls `IQuizRepository.GetQuestionsByLessonAsync(lessonId)` directly, maps to the admin DTO (with `IsCorrect`), no enrollment/progress checks at all — this is a pure content read for authorized staff.
   - `GetCourseExamQuestionsAdminQuery(Guid CourseId) : IQuery<IReadOnlyList<QuizQuestionAdminDto>>` → same, via `GetQuestionsByCourseAsync(courseId)`.
   - Put these in new files under `ELearning.Application/Features/Quizzes/Queries/` (e.g. `GetLessonQuizQuestionsAdminQuery.cs`, `GetCourseExamQuestionsAdminQuery.cs`), following the exact file-per-feature pattern already used by every other query in this codebase.
   - Handlers auto-register via reflection — see `ELearning.Application/DependencyInjection/DependencyInjectionExtensions.cs` (`RegisterQueryHandlers`), which scans the assembly for anything implementing `IQueryHandler<,>`. No manual DI wiring needed, just confirm it compiles and resolves.
3. `ELearning.API/Controllers/admin/AdminQuizzesController.cs` — inject the two new query handlers via constructor (primary-constructor style, matching the existing 6 command handlers already injected there) and add:
   - `[HttpGet("lessons/{lessonId:guid}")]` → `GET /api/admin/quizzes/lessons/{lessonId}`
   - `[HttpGet("courses/{courseId:guid}/exam")]` → `GET /api/admin/quizzes/courses/{courseId}/exam`
   Match the existing controller's style (`this.ToActionResult(result)`, no manual `User.GetUserId()` needed since these queries don't take a UserId).

### Frontend
1. `src/api/admin/quizzes.ts` — add `getLessonQuizzes(lessonId)` → `GET /admin/quizzes/lessons/{lessonId}` and `getCourseExam(courseId)` → `GET /admin/quizzes/courses/{courseId}/exam`, matching the existing `axios` call style in that file.
2. `src/hooks/admin/quizzes.ts` — add `useAdminLessonQuizzes(lessonId, enabled)` / `useAdminCourseExam(courseId, enabled)` query hooks, matching the shape/staleTime convention of `useLessonQuizzes`/`useCourseExam` in `src/hooks/quizzes.ts`.
3. `src/types/quiz.types.ts` — add whatever admin response type is needed so `isCorrect` is typed as required `boolean` (not optional) for the admin path, without touching the existing student-facing type that keeps it optional/absent.
4. `src/pages/admin/AdminQuizzesPage.tsx` — swap the import/usage at lines 11, 52-53 from `useLessonQuizzes`/`useCourseExam` (student hooks) to the new `useAdminLessonQuizzes`/`useAdminCourseExam` hooks. Verify the rest of the file (including `QuestionComposerModal.tsx` at lines 175/185 area) now receives real `isCorrect` values instead of `undefined`.

## Constraints / conventions
- Backend: xUnit + Moq for the 2 new handlers, mirror `ELearning.Tests/Unit/Aplication/Features/Quizzes/GetCourseExamHandlerTests.cs` (the student-facing equivalent) for style, but these tests should NOT need any enrollment mocking at all — that's the point of the fix. Cover: empty list when no questions, correct DTO mapping including `IsCorrect`, options ordered by `OrderIndex`.
- Controller pattern reference: `ELearning.API/Controllers/admin/AdminQuizzesController.cs` itself (already open, just extend it).
- Frontend: no test runner configured — verify with `npx tsc -b --noEmit` (must stay clean) from `src/frontend/elearning-web`.
- Do not modify `QuizQuestionDto`/`QuizOptionDto` (the student-facing ones) — new admin-only DTOs only.

## TDD mode
Strict TDD, real RED→GREEN (this is new production code):
- RED: write the 2 new handler tests first — since the handler classes don't exist yet, this will be a compile failure, which is valid RED evidence for genuinely new code (there's nothing to stub-throw here, unlike the Notifications case).
- GREEN: implement, re-run, confirm pass.
- Runner: `dotnet test ELearning.Tests/ELearning.Tests.csproj` from `src/backend`.

## Delivery strategy
Single work unit, one commit, Conventional Commits, no AI attribution. Branch from `main`: `fix/admin-quiz-questions-403`.

## Tasks
- [x] 1. Admin DTOs (`QuizQuestionAdminDto`/`QuizOptionAdminDto` with `IsCorrect`)
- [x] 2. `GetLessonQuizQuestionsAdminQuery` + handler + tests
- [x] 3. `GetCourseExamQuestionsAdminQuery` + handler + tests
- [x] 4. `AdminQuizzesController` GET endpoints wired
- [x] 5. Frontend api/hook additions + `AdminQuizzesPage.tsx` swapped to admin hooks
- [x] 6. `npx tsc -b --noEmit` clean, full backend suite green

## Progress

### RED (before implementation)
Ran `dotnet test ELearning.Tests/ELearning.Tests.csproj --filter "FullyQualifiedName~Admin"` from `src/backend` with only the two new test files present (handler classes not yet created):

```
error CS0246: El nombre del tipo o del espacio de nombres 'GetCourseExamQuestionsAdminHandler' no se encontró
error CS0246: El nombre del tipo o del espacio de nombres 'GetLessonQuizQuestionsAdminHandler' no se encontró
```
Compile failure — valid RED evidence for genuinely new code.

### GREEN (after implementation)
Same filtered command after implementing DTOs, queries/handlers, and controller endpoints:

```
Correctas! - Con error:     0, Superado:    17, Omitido:     0, Total:    17, Duración: 127 ms - ELearning.Tests.dll (net10.0)
```
(17 total = the 6 new tests + other pre-existing tests whose fully-qualified name contains "Admin", e.g. other Admin* handler tests.)

### Full suite (final)
`dotnet test ELearning.Tests/ELearning.Tests.csproj` (no filter), from `src/backend`:

```
Correctas! - Con error:     0, Superado:   541, Omitido:     0, Total:   541, Duración: 439 ms - ELearning.Tests.dll (net10.0)
```
541 total = 535 pre-existing + 6 new (3 per new handler test file). Note: the original estimate of "10 new tests" in this doc's TDD section was a planning guess; actual coverage (empty list, validation, full DTO mapping incl. `IsCorrect` + option ordering) needed 3 tests per handler, 6 total — all listed constraints are covered.

### Frontend
`npx tsc -b --noEmit` from `src/frontend/elearning-web`: clean, no output, exit 0.

### Student-facing code left untouched
Verified via `git diff --stat` against `GetCourseExamQuery.cs`, `GetLessonQuizzesQuery.cs`, `QuizQuestionDto.cs`, `QuizOptionDto.cs` — no diff on any of them. The new admin handlers depend on `IQuizRepository` only (no `IEnrollmentRepository`/`ICourseRepository`/`ILessonRepository`), confirming no enrollment-gating logic was copied into the fix.
