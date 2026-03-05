using ELearning.Application.Common.Abstractions;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Lessons.Commands;

public sealed record CreateLessonCommand(
    Guid CourseId,
    string Title,
    string Type,        // "video", "pdf", "quiz"
    string? ContentUrl,
    bool IsRequired,
    Guid RequesterId,
    string RequesterRole
) : ICommand<Guid>;

public sealed class CreateLessonHandler : ICommandHandler<CreateLessonCommand, Guid>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public CreateLessonHandler(
        ICourseRepository courses,
        ILessonRepository lessons)
    {
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateLessonCommand cmd,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(cmd.CourseId, ct);
        if (course is null)
            return Result.NotFound<Guid>($"Curso con id '{cmd.CourseId}' no encontrado.");

        var requesterRole = Enum.Parse<UserRole>(cmd.RequesterRole, ignoreCase: true);
        if (requesterRole == UserRole.Instructor && course.CreatedBy != cmd.RequesterId)
            return Result.Forbidden<Guid>("Solo el instructor que creó el curso puede agregar lecciones.");

        var lessonType = Enum.Parse<LessonType>(cmd.Type, ignoreCase: true);
        var maxOrder = await _lessons.GetMaxOrderIndexAsync(cmd.CourseId, ct);

        var lesson = Lesson.Create(
            courseId: cmd.CourseId,
            title: cmd.Title.Trim(),
            type: lessonType,
            contentUrl: cmd.ContentUrl,
            orderIndex: maxOrder + 1,  // siempre se agrega al final
            isRequired: cmd.IsRequired
        );

        await _lessons.CreateAsync(lesson, ct);

        return Result.Success(lesson.Id);
    }
}
