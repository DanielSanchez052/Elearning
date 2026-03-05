using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.DTOs;
using ELearning.Domain.Interfaces.Repositories;

namespace ELearning.Application.Features.Courses.Queries;

public sealed record GetCourseCatalogQuery(
    int CountryId,   // extraído del JWT — filtra cursos del país del usuario
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<CourseSummaryDto>>;

public sealed class GetCourseCatalogHandler
    : IQueryHandler<GetCourseCatalogQuery, PagedResult<CourseSummaryDto>>
{
    private readonly ICourseRepository _courses;
    private readonly ILessonRepository _lessons;

    public GetCourseCatalogHandler(
        ICourseRepository courses,
        ILessonRepository lessons)
    {
        _courses = courses;
        _lessons = lessons;
    }

    public async Task<Result<PagedResult<CourseSummaryDto>>> HandleAsync(GetCourseCatalogQuery query, CancellationToken ct = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var (courses, totalCount) = await _courses.GetCatalogAsync(
            countryId: query.CountryId,
            search: query.Search,
            page: page,
            pageSize: pageSize,
            ct: ct);

        //TODO: Revisar esto ya que puede ser problematico 
        // Obtener conteo de lecciones por curso en paralelo
        var lessonCounts = await Task.WhenAll(
            courses.Select(async c =>
            {
                var lessons = await _lessons.GetByCourseAsync(c.Id, ct);
                return (CourseId: c.Id, lessons.Count);
            }));

        var lessonCountMap = lessonCounts.ToDictionary(x => x.CourseId, x => x.Count);

        var dtos = courses.Select(c => new CourseSummaryDto(
            Id: c.Id,
            Title: c.Title,
            Description: c.Description,
            ThumbnailUrl: c.ThumbnailUrl,
            IsGlobal: c.IsGlobal,
            IsActive: c.IsActive,
            InstructorName: c.CreatedByUser.FullName,
            LessonCount: lessonCountMap.GetValueOrDefault(c.Id),
            CreatedAt: c.CreatedAt
        )).ToList().AsReadOnly();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result.Success(new PagedResult<CourseSummaryDto>(
            Items: dtos,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: totalPages
        ));
    }
}
