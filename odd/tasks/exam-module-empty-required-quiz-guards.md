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
- [ ] **T1** `DeleteQuizQuestionCommand.cs` (`src/backend/ELearning.Application/Features/Quizzes/Commands/DeleteQuizQuestionCommand.cs`) —
  before deleting, look up how many questions currently exist in the same
  scope as the question being deleted (its `LessonId` if set, else its
  `CourseId` via `_quizzes.GetQuestionsByLessonAsync`/`GetQuestionsByCourseAsync`).
  If this question is the only one in that scope, return
  `Result.ValidationFailure("No puedes eliminar la última pregunta de este quiz. Elimina la lección completa (o deja de usar el examen del curso) en su lugar.")`
  instead of deleting.
- [ ] **T2** `DeleteQuizOptionCommand.cs` (same folder) — before deleting, fetch
  all options for `option.QuestionId` via `_quizzes.GetOptionsByQuestionAsync`.
  Reject with `Result.ValidationFailure(...)` if either:
  - this is the only remaining option for the question (`count == 1`), or
  - this option `IsCorrect` and it's the only correct one remaining among the
    other options (i.e. deleting it would leave zero correct options).
  Two distinct messages for the two cases (e.g. "No puedes eliminar la última
  opción de esta pregunta." / "No puedes eliminar la única opción correcta de
  esta pregunta.").
- [ ] **T3** `CreateLessonCommand.cs` (`src/backend/ELearning.Application/Features/Lessons/Commands/CreateLessonCommand.cs`) —
  if `lessonType == LessonType.Quiz && cmd.IsRequired`, return
  `Result.ValidationFailure<Guid>("No puedes crear una lección de tipo Quiz como obligatoria sin preguntas. Créala primero, agrega sus preguntas, y luego márcala como obligatoria.")`
  instead of creating it. (A brand-new lesson can never have questions yet —
  this is always true at creation time for this type, no repository call
  needed.)
- [ ] **T4** `UpdateLessonCommand.cs` (`src/backend/ELearning.Application/Features/Lessons/Commands/UpdateLessonCommand.cs`) —
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
(fill in as each task completes: what was found, RED/GREEN evidence, full
suite count before/after)
