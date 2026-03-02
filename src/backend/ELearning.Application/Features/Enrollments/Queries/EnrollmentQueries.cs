using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.DTOs;

namespace ELearning.Application.Features.Enrollments.Queries;

public class GetMyEnrollmentsQuery : IQuery<List<EnrollmentDto>> { }
public class GetMyEnrollmentsHandler : IQueryHandler<GetMyEnrollmentsQuery, List<EnrollmentDto>> { 
    Task<Result<List<EnrollmentDto>>> IQueryHandler<GetMyEnrollmentsQuery, List<EnrollmentDto>>.HandleAsync(GetMyEnrollmentsQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class GetEnrollmentProgressQuery : IQuery<EnrollmentDto> { public Guid CourseId { get; set; } }
public class GetEnrollmentProgressHandler : IQueryHandler<GetEnrollmentProgressQuery, EnrollmentDto> { 
    Task<Result<EnrollmentDto>> IQueryHandler<GetEnrollmentProgressQuery, EnrollmentDto>.HandleAsync(GetEnrollmentProgressQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
