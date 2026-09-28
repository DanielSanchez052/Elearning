# Exam module: prevent required quiz lessons/questions from becoming unsatisfiable

## Objective
User reported a critical bug: the course final-exam endpoint (`courses/{courseId}/exam`)
was permanently unreachable for "Excel Avanzado". Root-caused (not a lesson-type
filtering bug — investigated and ruled out): the course has a `Lesson` row of
`Type = Quiz` ("Quiz – Capítulo 1"), marked `IsRequired = true`, with **zero**
`QuizQuestion`s ever attached to it. Since a Quiz-type lesson is only marked
complete by passing its own embedded quiz (`SubmitQuizCommand` with `LessonId`,
which the frontend follows up with `MarkLessonComplete` — this part of the
architecture is correct and intentional), a required Quiz-type lesson with no
questions can never be completed by any student, permanently blocking the
course's final exam and course completion.

**Dev-data fix already applied** (outside this task, direct DB update): the
broken lesson was unmarked as not-required.

**This task**: prevent the same silently-unsatisfiable-required-gate class of
bug from recurring anywhere in the Quizzes/Lessons module, found during a full
audit of the module requested by the user. Three gaps, all missing basic
invariant checks on destructive/structural operations:

1. `DeleteQuizQuestionCommand` — deleting the last remaining question for a
   lesson-quiz or course-exam leaves it question-less with no guard.
2. `DeleteQuizOptionCommand` — deleting an option can leave a question with 0
   options, or with 0 *correct* options (unanswerable-correctly forever).
3. `CreateLessonCommand` / `UpdateLessonCommand` — a `Quiz`-type lesson can be
   created or marked `IsRequired = true` with zero questions attached, with no
   validation — this is exactly how "Quiz – Capítulo 1" ended up broken.

## Scope decisions (already made, don't re-litigate)
- Do **not** change the "required lessons" completion-check filter itself
  (`GetCourseExamHandler`, `StartCourseExamCommand`, `SubmitQuizCommand`,
  `MarkLessonCompleteHandler` all do `Course.Lessons.Where(l => l.IsRequired)`
  uniformly across lesson types) — this is correct as-is, confirmed with the
  user. Quiz-type lessons legitimately count as required lessons; they're just
  completed via a different action (passing their quiz) than Video/Pdf
  (clicking "Marcar completada").
- Fix by rejecting the operation that WOULD create the broken state, with a
  clear validation error — not by silently overriding what the admin asked for.

## Tasks
- [x] **T1** `DeleteQuizQuestionCommand.cs` (`src/backend/ELearning.Application/Features/Quizzes/Commands/DeleteQuizQuestionCommand.cs`) —
  before deleting, look up how many questions currently exist in the same
  scope as the question being deleted (its `LessonId` if set, else its
  `CourseId` via `_quizzes.GetQuestionsByLessonAsync`/`GetQuestionsByCourseAsync`).
  If this question is the only one in that scope, return
  `Result.ValidationFailure("No puedes eliminar la última pregunta de este quiz. Elimina la lección completa (o deja de usar el examen del curso) en su lugar.")`
  instead of deleting.
- [x] **T2** `DeleteQuizOptionCommand.cs` (same folder) — before deleting, fetch
  all options for `option.QuestionId` via `_quizzes.GetOptionsByQuestionAsync`.
  Reject with `Result.ValidationFailure(...)` if either:
  - this is the only remaining option for the question (`count == 1`), or
  - this option `IsCorrect` and it's the only correct one remaining among the
    other options (i.e. deleting it would leave zero correct options).
  Two distinct messages for the two cases (e.g. "No puedes eliminar la última
  opción de esta pregunta." / "No puedes eliminar la única opción correcta de
  esta pregunta.").
- [x] **T3** `CreateLessonCommand.cs` (`src/backend/ELearning.Application/Features/Lessons/Commands/CreateLessonCommand.cs`) —
  if `lessonType == LessonType.Quiz && cmd.IsRequired`, return
  `Result.ValidationFailure<Guid>("No puedes crear una lección de tipo Quiz como obligatoria sin preguntas. Créala primero, agrega sus preguntas, y luego márcala como obligatoria.")`
  instead of creating it. (A brand-new lesson can never have questions yet —
  this is always true at creation time for this type, no repository call
  needed.)
- [x] **T4** `UpdateLessonCommand.cs` (`src/backend/ELearning.Application/Features/Lessons/Commands/UpdateLessonCommand.cs`) —
  needs `IQuizRepository` injected (not currently a dependency — add it to the
  constructor and to wherever this handler is constructed/registered, check
  the DI registration scan still picks it up automatically via the existing
  `ICommandHandler<,>` reflection scan in `DependencyInjectionExtensions.cs`,
  it should since that only reflects handler constructors). If the lesson's
  `Type == LessonType.Quiz` and `cmd.IsRequired == true` and the lesson
  currently has zero questions (`_quizzes.GetQuestionsByLessonAsync(lesson.Id, ct)`
  count == 0), return
  `Result.ValidationFailure("No puedes marcar esta lección como obligatoria porque no tiene preguntas. Agrega al menos una pregunta primero.")`
  instead of updating. Editing a non-Quiz lesson, or setting `IsRequired = false`,
  or a Quiz lesson that already has questions, is unaffected.

## TDD mode
Strict (user's global default), real RED→GREEN for all 4 — these are new
production code paths (new validation branches), not characterization of
existing behavior.

## Delivery strategy
One work unit, one commit (all 4 are the same investigation and the same
narrow class of bug). Branch: `fix/exam-module-empty-required-quiz-guards`
from `main`.

## Progress

**Baseline**: `dotnet test ELearning.Tests/ELearning.Tests.csproj` from `src/backend` →
`Con error: 0, Superado: 641, Omitido: 0, Total: 641` (confirmed before starting,
matches the ~641 estimate).

All 4 handlers already had test files with NotFound/Forbidden/happy-path coverage
(`DeleteQuizQuestionHandlerTests`, `DeleteQuizOptionHandlerTests`,
`CreateLessonHandlerTests`, `UpdateLessonHandlerTests`), so no new empty-file
scaffolding was needed — only the new guard tests (and, for T1/T2, updating the
existing happy-path test to satisfy the new repository call the guard requires).

**T1 — `DeleteQuizQuestionCommand.cs`**
- RED: added `HandleAsync_LastQuestionInLesson_ReturnsValidationFailureAndDoesNotDelete`
  and `HandleAsync_LastQuestionInCourseExam_ReturnsValidationFailureAndDoesNotDelete`
  (mocking `GetQuestionsByLessonAsync`/`GetQuestionsByCourseAsync` to return a
  single-question list). Ran against unguarded code: both failed
  (`Assert.False() Failure: Expected False, Actual True` — deletion succeeded).
- GREEN: added scope lookup (`LessonId` → `GetQuestionsByLessonAsync`, else
  `CourseId` → `GetQuestionsByCourseAsync`) before delete; if
  `scopeQuestions.Count <= 1`, return
  `Result.ValidationFailure("No puedes eliminar la última pregunta de este quiz. Elimina la lección completa (o deja de usar el examen del curso) en su lugar.")`.
  Updated the existing happy-path test to mock a 2-question scope. Both new
  tests pass.

**T2 — `DeleteQuizOptionCommand.cs`**
- RED: added `HandleAsync_LastRemainingOption_...` and
  `HandleAsync_LastCorrectOption_...`. Both failed against unguarded code the
  same way (deletion succeeded when it shouldn't).
- GREEN: fetch `GetOptionsByQuestionAsync(option.QuestionId)`; reject with
  `Result.ValidationFailure("No puedes eliminar la última opción de esta pregunta.")`
  if `options.Count <= 1`; reject with
  `Result.ValidationFailure("No puedes eliminar la única opción correcta de esta pregunta.")`
  if the option `IsCorrect` and it's the only correct one left. Added
  `using System.Linq;` for `.Count(predicate)`. Updated the existing happy-path
  test to mock 2 options (one correct, one not). Both new tests pass.

**T3 — `CreateLessonCommand.cs`**
- RED: added `HandleAsync_RequiredQuizLesson_ReturnsValidationFailureAndDoesNotCreate`
  (Type "quiz", IsRequired true). Failed against unguarded code (lesson was
  created).
- GREEN: after parsing `lessonType`, if `lessonType == LessonType.Quiz &&
  cmd.IsRequired`, return
  `Result.ValidationFailure<Guid>("No puedes crear una lección de tipo Quiz como obligatoria sin preguntas. Créala primero, agrega sus preguntas, y luego márcala como obligatoria.")`
  before touching the lesson/order-index repository calls (no repo call
  needed, per the doc's note that a new lesson can never have questions yet).
  Test passes.

**T4 — `UpdateLessonCommand.cs`**
- Confirmed `IQuizRepository` is registered in
  `ELearning.Infrastructure/DependencyInjection/DependencyInjectionExtensions.cs`,
  and `ELearning.Application`'s `RegisterCommandHandlers` registers handler
  types via plain `services.AddScoped(handlerType)` (constructor-injected by
  the container, not a hand-written factory), so adding the new
  `IQuizRepository` constructor parameter to `UpdateLessonHandler` is picked
  up automatically — **no change to `DependencyInjectionExtensions.cs` was
  needed** (verified, not assumed).
- RED: adding `_quizzesMock` + passing it as a 3rd constructor arg in
  `UpdateLessonHandlerTests` first produced a compile error
  (`CS1729: 'UpdateLessonHandler' no contiene un constructor que tome 3
  argumentos`) since the constructor didn't accept it yet — the real RED for
  this task's shape. After adding the constructor parameter (mechanical,
  no guard logic), the new test
  `HandleAsync_QuizLessonWithoutQuestionsMarkedRequired_...` failed at runtime
  (`Assert.False() Failure: Expected False, Actual True` — update succeeded
  when it shouldn't have).
- GREEN: after the ownership/forbidden check, if `lesson.Type ==
  LessonType.Quiz && cmd.IsRequired`, call
  `_quizzes.GetQuestionsByLessonAsync(lesson.Id, ct)`; if count == 0, return
  `Result.ValidationFailure("No puedes marcar esta lección como obligatoria porque no tiene preguntas. Agrega al menos una pregunta primero.")`.
  Added a companion `HandleAsync_QuizLessonWithQuestionsMarkedRequired_UpdatesSuccessfully`
  test (Quiz lesson with 1 question, marked required → succeeds) to cover the
  non-blocked Quiz path. Both new tests pass; non-Quiz and `IsRequired=false`
  paths are unaffected (existing tests still green).

**Final suite**: `Con error: 0, Superado: 648, Omitido: 0, Total: 648` (641 baseline
+ 7 new tests: 2 for T1, 2 for T2, 1 for T3, 2 for T4). No regressions.

**Commit**: `0aaaaf8` — `fix(quizzes,lessons): guard against unsatisfiable required-quiz states`
(no push, no PR, per instructions).

**T1 refinement — narrowed after Gentle AI review feedback (post-commit `0aaaaf8`)**

The `review-reliability` lens (medium-risk candidate covering the guards commit)
flagged the T1 guard as overbroad: it blocked deleting the last question for
*any* lesson-quiz (even a non-required one) and for the course-exam scope,
neither of which can actually leave an unsatisfiable required gate — a
non-required Quiz lesson emptying out doesn't block anything, and the course
exam's "requiredness" is itself derived from having ≥1 question
(`hasFinalExam = GetQuestionsByCourseAsync(...).Count > 0`, see
`MarkLessonCompleteCommand.cs`), so removing its last question just turns the
final exam off — it doesn't strand a student.

Narrowed the guard to only fire when `question.LessonId` is set AND the
owning lesson (`ILessonRepository.GetByIdAsync`) has `IsRequired == true`.
Course-exam-scoped questions (`LessonId is null`) are no longer guarded at
all by this handler.

- RED: renamed `HandleAsync_LastQuestionInLesson_...` to
  `HandleAsync_LastQuestionInRequiredLesson_ReturnsValidationFailureAndDoesNotDelete`
  (now stubs a required `Lesson` via `ILessonRepository`); added
  `HandleAsync_LastQuestionInNonRequiredLesson_DeletesSuccessfully`; flipped
  `HandleAsync_LastQuestionInCourseExam_...` to expect success
  (`DeletesSuccessfully`). Compile RED first (`CS1729`, constructor now takes
  2 args), then runtime RED for the new non-required/course-exam cases
  against the old unconditional guard.
- GREEN: injected `ILessonRepository` into `DeleteQuizQuestionHandler`
  (already DI-registered elsewhere, picked up automatically, same as T4's
  `UpdateLessonHandler`); guard now scoped to
  `question.LessonId is not null && lesson is { IsRequired: true }`.
- **Final suite**: `Con error: 0, Superado: 649, Omitido: 0, Total: 649` (648 + 1
  net new test). No regressions.
- **Not done in this pass** (deferred, low impact per user): the TOCTOU race
  the same lens flagged (read-count-then-delete, no lock/second check) on
  both `DeleteQuizQuestionCommand` and `DeleteQuizOptionCommand` — real but
  requires literally concurrent admin requests on the last 2 items; noted as
  future hardening, not fixed here.
