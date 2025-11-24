using System.Reflection;
using CacheDemo.Application.Common.Mediator;
using CacheDemo.Application.Common.Mediator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CacheDemo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<IMediator, Mediator>();

        // Register all handlers
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var @interface in interfaces) services.AddScoped(@interface, handlerType);
        }

        return services;
    }
}