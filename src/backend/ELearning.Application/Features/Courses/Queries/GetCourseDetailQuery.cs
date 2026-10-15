using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Authorization;
using ELearning.Application.Features.Countries.Dtos;
using ELearning.Application.Features.Courses.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Queries;

public sealed record GetCourseDetailQuery(
    Guid CourseId,
    Guid RequesterId,
    string RequesterRole,
    int RequesterCountryId
) : IQuery<CourseDetailDto>;

/// <summary>
/// Quién ve el detalle de un curso:
/// <list type="bullet">
///   <item>quien lo gestiona (ver <see cref="CourseAccess.CanManage"/>), en cualquier estado;</item>
///   <item>quien está inscrito, aunque después se despublique o cambie de país;</item>
///   <item>el resto, solo si está publicado y disponible en su país (como en el catálogo).</item>
/// </list>
/// Si no le corresponde, responde 404 para no revelar que el curso existe.
/// Los enlaces al contenido de las lecciones solo van a quien lo gestiona o está inscrito.
/// </summary>
public sealed class GetCourseDetailHandler
    : IQueryHandler<GetCourseDetailQuery, CourseDetailDto>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;
    private readonly IEnrollmentRepository _enrollments;

    public GetCourseDetailHandler(
        ICourseRepository courses,
        ILessonRepository lessons,
        IEnrollmentRepository enrollments)
    {
        _courses = courses;
        _lessons = lessons;
        _enrollments = enrollments;
    }

    public async Task<Result<CourseDetailDto>> HandleAsync(
        GetCourseDetailQuery query,
        CancellationToken ct = default)
    {
        var notFound = Result.NotFound<CourseDetailDto>(
            $"Curso con id '{query.CourseId}' no encontrado.");

        var course = await _courses.GetByIdAsync(query.CourseId, ct);
        if (course is null)
            return notFound;

        var canManage = CourseAccess.CanManage(
            course, query.RequesterId, query.RequesterRole, query.RequesterCountryId);
        var isEnrolled = !canManage
            && await _enrollments.ExistsAsync(query.RequesterId, query.CourseId, ct);

        if (!canManage && !isEnrolled && !CourseAccess.IsAvailableIn(course, query.RequesterCountryId))
            return notFound;

        var showContent = canManage || isEnrolled;

        var lessons = await _lessons.GetByCourseAsync(query.CourseId, ct);
        var countries = await _courses.GetCourseCountriesAsync(query.CourseId, ct);

        var lessonDtos = lessons.Select(l => new LessonDto(
            Id: l.Id,
            Title: l.Title,
            Type: l.Type.ToString().ToLowerInvariant(),
            ContentUrl: showContent ? l.ContentUrl : null,
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
