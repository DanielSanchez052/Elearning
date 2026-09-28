using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Lessons.Commands;

public sealed record UpdateLessonCommand(
    Guid LessonId,
    string Title,
    string? ContentUrl,
    bool IsRequired,
    Guid RequesterId,
    string RequesterRole
) : ICommand;

public sealed class UpdateLessonHandler : ICommandHandler<UpdateLessonCommand>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;
    private readonly IQuizRepository _quizzes;

    public UpdateLessonHandler(
        ICourseRepository courses,
        ILessonRepository lessons,
        IQuizRepository quizzes)
    {
        _courses = courses;
        _lessons = lessons;
        _quizzes = quizzes;
    }

    public async Task<Result> HandleAsync(
        UpdateLessonCommand cmd,
        CancellationToken ct = default)
    {
        var lesson = await _lessons.GetByIdTrackedAsync(cmd.LessonId, ct);
        if (lesson is null)
            return Result.NotFound($"Lección con id '{cmd.LessonId}' no encontrada.");

        var course = await _courses.GetByIdAsync(lesson.CourseId, ct);
        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course!.CreatedBy != cmd.RequesterId)
            return Result.Forbidden("Solo el instructor que creó el curso puede editar sus lecciones.");

        if (lesson.Type == LessonType.Quiz && cmd.IsRequired)
        {
            var questions = await _quizzes.GetQuestionsByLessonAsync(lesson.Id, ct);
            if (questions.Count == 0)
                return Result.ValidationFailure("No puedes marcar esta lección como obligatoria porque no tiene preguntas. Agrega al menos una pregunta primero.");
        }

        lesson.Update(
            title: cmd.Title.Trim(),
            contentUrl: cmd.ContentUrl,
            isRequired: cmd.IsRequired
        );

        await _lessons.UpdateAsync(lesson, ct);

        return Result.Success();
    }
}