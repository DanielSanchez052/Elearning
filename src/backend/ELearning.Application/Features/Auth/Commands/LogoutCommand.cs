using ELearning.Application.Common.Abstractions;

namespace ELearning.Application.Features.Auth.Commands;

public class LogoutCommand : ICommand
{
}

public class LogoutHandler : ICommandHandler<LogoutCommand>
{
    Task<Result> ICommandHandler<LogoutCommand>.HandleAsync(LogoutCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
