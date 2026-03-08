using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Quizzes.Commands;

public sealed record CreateQuizQuestionCommand(
    Guid? LessonId,
    Guid? CourseId,
    int Type,
    string QuestionText,
    decimal PassScore,
    int MaxAttempts,
    bool IsRequired
) : ICommand<Guid>;

public sealed class CreateQuizQuestionHandler : ICommandHandler<CreateQuizQuestionCommand, Guid>
{
    private readonly ILessonRepository _lessons;
    private readonly ICourseRepository _courses;
    private readonly IQuizRepository _quizzes;

    public CreateQuizQuestionHandler(
        ILessonRepository lessons,
        ICourseRepository courses,
        IQuizRepository quizzes)
    {
        _lessons = lessons;
        _courses = courses;
        _quizzes = quizzes;
    }

    public async Task<Result<Guid>> HandleAsync(CreateQuizQuestionCommand cmd, CancellationToken ct = default)
    {
        var quizType = (QuizType)cmd.Type;

        if (quizType == QuizType.PerLesson)
        {
            if (cmd.LessonId == Guid.Empty)
                return Result.ValidationFailure<Guid>("LessonId es requerido para quizzes por lección");

            var lesson = await _lessons.GetByIdAsync(cmd.LessonId.Value, ct);
            if (lesson is null)
                return Result.NotFound<Guid>($"Lección con id '{cmd.LessonId}' no encontrada");

            var question = QuizQuestion.CreatePerLesson(
                cmd.LessonId.Value,
                cmd.QuestionText,
                cmd.PassScore,
                cmd.MaxAttempts,
                1,
                cmd.IsRequired
            );

            await _quizzes.CreateQuestionAsync(question, ct);
            await _quizzes.SaveChangesAsync(ct);

            return question.Id;
        }
        else if (quizType == QuizType.CourseExam)
        {
            if (cmd.CourseId == Guid.Empty)
                return Result.ValidationFailure<Guid>("CourseId es requerido para exámenes de curso");

            var course = await _courses.GetByIdAsync(cmd.CourseId.Value, ct);
            if (course is null)
                return Result.NotFound<Guid>($"Curso con id '{cmd.CourseId}' no encontrado");

            var question = QuizQuestion.CreateCourseExam(
                cmd.CourseId.Value,
                cmd.QuestionText,
                cmd.PassScore,
                cmd.MaxAttempts,
                1,
                cmd.IsRequired
            );

            await _quizzes.CreateQuestionAsync(question, ct);
            await _quizzes.SaveChangesAsync(ct);

            return question.Id;
        }

        return Result.ValidationFailure<Guid>("Tipo de quiz inválido");
    }
}
