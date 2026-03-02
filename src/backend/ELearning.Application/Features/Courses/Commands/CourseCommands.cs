using ELearning.Application.Common.Abstractions;

namespace ELearning.Application.Features.Courses.Commands;

public class CreateCourseCommand : ICommand<Guid>
{
}

public class CreateCourseHandler : ICommandHandler<CreateCourseCommand, Guid>
{

    Task<Result<Guid>> ICommandHandler<CreateCourseCommand, Guid>.HandleAsync(CreateCourseCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class UpdateCourseCommand : ICommand
{
}

public class UpdateCourseHandler : ICommandHandler<UpdateCourseCommand>
{

    Task<Result> ICommandHandler<UpdateCourseCommand>.HandleAsync(UpdateCourseCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class DeleteCourseCommand : ICommand
{
}

public class DeleteCourseHandler : ICommandHandler<DeleteCourseCommand>
{

    Task<Result> ICommandHandler<DeleteCourseCommand>.HandleAsync(DeleteCourseCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class ActivateCourseCommand : ICommand
{
}

public class ActivateCourseHandler : ICommandHandler<ActivateCourseCommand>
{
    Task<Result> ICommandHandler<ActivateCourseCommand>.HandleAsync(ActivateCourseCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
