using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FortressIdentity.Application;

/// <summary>
/// Extension methods for configuring Application services in Dependency Injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services into the DI container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR - handles commands and queries
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });

        // Register FluentValidation - validates commands and queries
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
