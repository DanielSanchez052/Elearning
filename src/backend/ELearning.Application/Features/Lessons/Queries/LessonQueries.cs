using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Lessons.DTOs;

namespace ELearning.Application.Features.Lessons.Queries;

public class GetLessonByIdQuery : IQuery<LessonDto> { public Guid LessonId { get; set; } }
public class GetLessonByIdHandler : IQueryHandler<GetLessonByIdQuery, LessonDto> { 
    Task<Result<LessonDto>> IQueryHandler<GetLessonByIdQuery, LessonDto>.HandleAsync(GetLessonByIdQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
