using ELearning.Application.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace ELearning.Application.Common.Decorators;

public sealed class LoggingCommandDecorator<TCommand, TResponse>
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _inner;
    private readonly ILogger<LoggingCommandDecorator<TCommand, TResponse>> _logger;

    public LoggingCommandDecorator(
        ICommandHandler<TCommand, TResponse> inner,
        ILogger<LoggingCommandDecorator<TCommand, TResponse>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse>> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var name = typeof(TCommand).Name;
        _logger.LogInformation("[Command] Starting {CommandName}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _inner.HandleAsync(command, ct);
            sw.Stop();
            if (result.IsSuccess)
                _logger.LogInformation("[Command] {CommandName} succeeded in {Ms}ms", name, sw.ElapsedMilliseconds);
            else
                _logger.LogWarning("[Command] {CommandName} failed in {Ms}ms: {Error}", name, sw.ElapsedMilliseconds, result.Error);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[Command] {CommandName} threw after {Ms}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}

public sealed class LoggingCommandDecorator<TCommand>
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _inner;
    private readonly ILogger<LoggingCommandDecorator<TCommand>> _logger;

    public LoggingCommandDecorator(
        ICommandHandler<TCommand> inner,
        ILogger<LoggingCommandDecorator<TCommand>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var name = typeof(TCommand).Name;
        _logger.LogInformation("[Command] Starting {CommandName}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _inner.HandleAsync(command, ct);
            sw.Stop();
            if (result.IsSuccess)
                _logger.LogInformation("[Command] {CommandName} succeeded in {Ms}ms", name, sw.ElapsedMilliseconds);
            else
                _logger.LogWarning("[Command] {CommandName} failed in {Ms}ms: {Error}", name, sw.ElapsedMilliseconds, result.Error);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[Command] {CommandName} threw after {Ms}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}

public sealed class LoggingQueryDecorator<TQuery, TResponse>
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _inner;
    private readonly ILogger<LoggingQueryDecorator<TQuery, TResponse>> _logger;

    public LoggingQueryDecorator(
        IQueryHandler<TQuery, TResponse> inner,
        ILogger<LoggingQueryDecorator<TQuery, TResponse>> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken ct = default)
    {
        var name = typeof(TQuery).Name;
        _logger.LogInformation("[Query] Starting {QueryName}", name);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var result = await _inner.HandleAsync(query, ct);
            sw.Stop();
            if (result.IsSuccess)
                _logger.LogInformation("[Query] {QueryName} succeeded in {Ms}ms", name, sw.ElapsedMilliseconds);
            else
                _logger.LogWarning("[Query] {QueryName} failed in {Ms}ms: {Error}", name, sw.ElapsedMilliseconds, result.Error);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[Query] {QueryName} threw after {Ms}ms", name, sw.ElapsedMilliseconds);
            throw;
        }
    }
}