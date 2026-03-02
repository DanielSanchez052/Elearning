using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Courses.DTOs;

namespace ELearning.Application.Features.Courses.Queries;

public class GetCoursesCatalogQuery : IQuery<List<CourseDto>>
{
    public int CountryId { get; set; }
    public string? Search { get; set; }
}

public class GetCoursesCatalogHandler : IQueryHandler<GetCoursesCatalogQuery, List<CourseDto>>
{

    Task<Result<List<CourseDto>>> IQueryHandler<GetCoursesCatalogQuery, List<CourseDto>>.HandleAsync(GetCoursesCatalogQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class GetCourseByIdQuery : IQuery<CourseDetailDto>
{
    public Guid CourseId { get; set; }
}

public class GetCourseByIdHandler : IQueryHandler<GetCourseByIdQuery, CourseDetailDto>
{
    Task<Result<CourseDetailDto>> IQueryHandler<GetCourseByIdQuery, CourseDetailDto>.HandleAsync(GetCourseByIdQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class GetMyCoursesQuery : IQuery<List<CourseDto>>
{
}

public class GetMyCoursesHandler : IQueryHandler<GetMyCoursesQuery, List<CourseDto>>
{

    Task<Result<List<CourseDto>>> IQueryHandler<GetMyCoursesQuery, List<CourseDto>>.HandleAsync(GetMyCoursesQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
