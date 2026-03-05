using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Application.Features.Courses.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Queries;

public sealed record GetCourseDetailQuery( Guid CourseId) : IQuery<CourseDetailDto>;

public sealed class GetCourseDetailHandler
    : IQueryHandler<GetCourseDetailQuery, CourseDetailDto>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public GetCourseDetailHandler(ICourseRepository courses, ILessonRepository lessons)
    {
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result<CourseDetailDto>> HandleAsync(
        GetCourseDetailQuery query,
        CancellationToken ct = default)
    {
        var course = await _courses.GetByIdAsync(query.CourseId, ct);
        if (course is null)
            return Result.NotFound<CourseDetailDto>(
                $"Curso con id '{query.CourseId}' no encontrado.");

        var lessons = await _lessons.GetByCourseAsync(query.CourseId, ct);
        var countries = await _courses.GetCourseCountriesAsync(query.CourseId, ct);

        var lessonDtos = lessons.Select(l => new LessonDto(
            Id: l.Id,
            Title: l.Title,
            Type: l.Type.ToString().ToLowerInvariant(),
            ContentUrl: l.ContentUrl,
            OrderIndex: l.OrderIndex,
            IsRequired: l.IsRequired
        )).ToList().AsReadOnly();

        var countryDtos = countries.Select(cc => new CountryDto(
            cc.Country.Id,
            cc.Country.Code,
            cc.Country.Name,
            cc.Country.IsActive
        )).ToList().AsReadOnly();

        return Result.Success(new CourseDetailDto(
            Id: course.Id,
            Title: course.Title,
            Description: course.Description,
            ThumbnailUrl: course.ThumbnailUrl,
            IsGlobal: course.IsGlobal,
            IsActive: course.IsActive,
            InstructorName: course.CreatedByUser.FullName,
            InstructorId: course.CreatedBy,
            Lessons: lessonDtos,
            Countries: countryDtos,
            CreatedAt: course.CreatedAt,
            UpdatedAt: course.UpdatedAt
        ));
    }
}
