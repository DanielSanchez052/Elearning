 using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.Application.Common.Decorators;

public sealed class ValidationCommandDecorator<TCommand, TResponse>
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _inner;
    private readonly IValidator<TCommand>? _validator;

    public ValidationCommandDecorator(
        ICommandHandler<TCommand, TResponse> inner,
        IServiceProvider serviceProvider)
    {
        _inner = inner;
        _validator = serviceProvider.GetService<IValidator<TCommand>>();
    }

    public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken ct = default)
    {

        if (_validator is not null)
        {
            if (command is null)
            {
                var errors = "Request: El request no puede estar vacio";
                return Result.ValidationFailure<TResponse>(errors);
            }
            
            var validation = _validator.Validate(command);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                return Result.ValidationFailure<TResponse>(errors);
            }
        }

        return await _inner.HandleAsync(command, ct);
    }
}

public sealed class ValidationCommandDecorator<TCommand>
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly IValidator<TCommand>? _validator;

    public ValidationCommandDecorator(
        ICommandHandler<TCommand> inner,
        IServiceProvider serviceProvider)
    {
        _inner = inner;
        _validator = serviceProvider.GetService<IValidator<TCommand>>();
    }

    public async Task<Result> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        if (_validator is not null)
        {
            if (command is null)
            {
                var errors = "Request: El request no puede estar vacio";
                return Result.ValidationFailure(errors);
            }

            var validation = _validator.Validate(command);
            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors
                    .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
                return Result.ValidationFailure(errors);
            }
        }

        return await _inner.HandleAsync(command, ct);
    }
}