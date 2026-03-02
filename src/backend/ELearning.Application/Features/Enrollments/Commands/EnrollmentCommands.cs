using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Enrollments.DTOs;

namespace ELearning.Application.Features.Enrollments.Commands;

public class EnrollCourseCommand : ICommand<Guid> { public Guid CourseId { get; set; } }
public class EnrollCourseHandler : ICommandHandler<EnrollCourseCommand, Guid> { 
    Task<Result<Guid>> ICommandHandler<EnrollCourseCommand, Guid>.HandleAsync(EnrollCourseCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class CompleteLessonCommand : ICommand { public Guid LessonId { get; set; } }
public class CompleteLessonHandler : ICommandHandler<CompleteLessonCommand> { 
    Task<Result> ICommandHandler<CompleteLessonCommand>.HandleAsync(CompleteLessonCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
