using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;

namespace ELearning.Application.Features.Quizzes.Queries;

public class GetQuizForLessonQuery : IQuery<QuizQuestionDto> { public Guid LessonId { get; set; } }
public class GetQuizForLessonHandler : IQueryHandler<GetQuizForLessonQuery, QuizQuestionDto> { 

    Task<Result<QuizQuestionDto>> IQueryHandler<GetQuizForLessonQuery, QuizQuestionDto>.HandleAsync(GetQuizForLessonQuery query, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
