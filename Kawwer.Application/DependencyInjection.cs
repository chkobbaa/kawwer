using System.Reflection;
using FluentValidation;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kawwer.Application;

/// <summary>
/// Registers the Application layer: the dispatcher, every command/query handler, every
/// FluentValidation validator, and shared application services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<INotificationService, NotificationService>();

        RegisterImplementations(services, assembly, typeof(IRequestHandler<,>));
        RegisterImplementations(services, assembly, typeof(IValidator<>));

        return services;
    }

    private static void RegisterImplementations(IServiceCollection services, Assembly assembly, Type openInterface)
    {
        var types = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var type in types)
        {
            var closedInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterface);

            foreach (var closedInterface in closedInterfaces)
            {
                services.AddScoped(closedInterface, type);
            }
        }
    }
}
