using ELearning.Application.Common.Abstractions;
using ELearning.Application.Common.Decorators;
using ELearning.Application.Common.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.Application.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjectionExtensions).Assembly;

        RegisterCommandHandlers(services, assembly);
        RegisterQueryHandlers(services, assembly);
        RegisterValidators(services, assembly);

        return services;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMMANDS
    // ─────────────────────────────────────────────────────────────────────────

    private static void RegisterCommandHandlers(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        // Busca todas las clases concretas que implementen ICommandHandler<TCommand, TResponse>
        // o ICommandHandler<TCommand>
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
                .Select(i => (HandlerType: t, HandlerInterface: i)))
            .ToList();

        foreach (var (handlerType, handlerInterface) in commandHandlerTypes)
        {
            // 1. Registrar el handler real con su propio tipo concreto
            //    (los decorators lo resolverán por tipo concreto)
            services.AddScoped(handlerType);

            // 2. Registrar la cadena de decorators apuntando a la interface
            //    Orden de envoltura (de afuera hacia adentro):
            //      ValidationDecorator → LoggingDecorator → Handler real
            //
            //    Al resolver ICommandHandler<TCommand, TResponse>, el DI entrega
            //    el ValidationDecorator, que tiene adentro el LoggingDecorator,
            //    que tiene adentro el handler real.

            var genericArgs = handlerInterface.GetGenericArguments();

            services.AddScoped(handlerInterface, sp =>
            {
                // Resolver el handler real
                var realHandler = sp.GetRequiredService(handlerType);

                if (genericArgs.Length == 2)
                {
                    // ICommandHandler<TCommand, TResponse>
                    var tCommand  = genericArgs[0];
                    var tResponse = genericArgs[1];

                    // Envolver con LoggingDecorator
                    var loggingDecoratorType = typeof(LoggingCommandDecorator<,>)
                        .MakeGenericType(tCommand, tResponse);
                    var loggingDecorator = ActivatorUtilities
                        .CreateInstance(sp, loggingDecoratorType, realHandler);

                    // Envolver con ValidationDecorator (el más externo)
                    var validationDecoratorType = typeof(ValidationCommandDecorator<,>)
                        .MakeGenericType(tCommand, tResponse);
                    var validationDecorator = ActivatorUtilities
                        .CreateInstance(sp, validationDecoratorType, loggingDecorator);

                    return validationDecorator;
                }
                else
                {
                    // ICommandHandler<TCommand> (sin TResponse)
                    var tCommand = genericArgs[0];

                    var loggingDecoratorType = typeof(LoggingCommandDecorator<>)
                        .MakeGenericType(tCommand);
                    var loggingDecorator = ActivatorUtilities
                        .CreateInstance(sp, loggingDecoratorType, realHandler);

                    var validationDecoratorType = typeof(ValidationCommandDecorator<>)
                        .MakeGenericType(tCommand);
                    var validationDecorator = ActivatorUtilities
                        .CreateInstance(sp, validationDecoratorType, loggingDecorator);

                    return validationDecorator;
                }
            });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    private static void RegisterQueryHandlers(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        var queryHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                .Select(i => (HandlerType: t, HandlerInterface: i)))
            .ToList();

        foreach (var (handlerType, handlerInterface) in queryHandlerTypes)
        {
            services.AddScoped(handlerType);

            var genericArgs = handlerInterface.GetGenericArguments();
            var tQuery    = genericArgs[0];
            var tResponse = genericArgs[1];

            services.AddScoped(handlerInterface, sp =>
            {
                var realHandler = sp.GetRequiredService(handlerType);

                // Queries solo tienen LoggingDecorator (sin validación — ver nota en ValidationDecorator.cs)
                var loggingDecoratorType = typeof(LoggingQueryDecorator<,>)
                    .MakeGenericType(tQuery, tResponse);

                return ActivatorUtilities.CreateInstance(sp, loggingDecoratorType, realHandler);
            });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // VALIDATORS
    // ─────────────────────────────────────────────────────────────────────────

    private static void RegisterValidators(IServiceCollection services, System.Reflection.Assembly assembly)
    {
        // Registra automáticamente todas las clases que implementen IValidator<T>
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => (ValidatorType: t, ValidatorInterface: i)));

        foreach (var (validatorType, validatorInterface) in validatorTypes)
            services.AddScoped(validatorInterface, validatorType);
    }
}