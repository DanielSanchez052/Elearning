using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Lessons.DTOs;

namespace ELearning.Application.Features.Lessons.Commands;

public class CreateLessonCommand : ICommand<Guid> { }
public class CreateLessonHandler : ICommandHandler<CreateLessonCommand, Guid> { 
    Task<Result<Guid>> ICommandHandler<CreateLessonCommand, Guid>.HandleAsync(CreateLessonCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class UpdateLessonCommand : ICommand { }
public class UpdateLessonHandler : ICommandHandler<UpdateLessonCommand> {
    Task<Result> ICommandHandler<UpdateLessonCommand>.HandleAsync(UpdateLessonCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class DeleteLessonCommand : ICommand { }
public class DeleteLessonHandler : ICommandHandler<DeleteLessonCommand> { 
    Task<Result> ICommandHandler<DeleteLessonCommand>.HandleAsync(DeleteLessonCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class ReorderLessonsCommand : ICommand { }
public class ReorderLessonsHandler : ICommandHandler<ReorderLessonsCommand> { 
    Task<Result> ICommandHandler<ReorderLessonsCommand>.HandleAsync(ReorderLessonsCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
