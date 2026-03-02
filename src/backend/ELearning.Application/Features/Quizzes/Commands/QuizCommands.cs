using ELearning.Application.Common.Abstractions;
using ELearning.Application.Features.Quizzes.DTOs;

namespace ELearning.Application.Features.Quizzes.Commands;

public class CreateQuizCommand : ICommand<Guid> { }
public class CreateQuizHandler : ICommandHandler<CreateQuizCommand, Guid> { 
    Task<Result<Guid>> ICommandHandler<CreateQuizCommand, Guid>.HandleAsync(CreateQuizCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

public class SubmitQuizCommand : ICommand<decimal> { }
public class SubmitQuizHandler : ICommandHandler<SubmitQuizCommand, decimal> { 
    Task<Result<decimal>> ICommandHandler<SubmitQuizCommand, decimal>.HandleAsync(SubmitQuizCommand command, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
