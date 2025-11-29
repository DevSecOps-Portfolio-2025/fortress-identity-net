using FortressIdentity.Application.Common.Interfaces.Authentication;
using FortressIdentity.Application.Common.Interfaces.Persistence;
using FortressIdentity.Infrastructure.Authentication;
using FortressIdentity.Infrastructure.Persistence;
using FortressIdentity.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FortressIdentity.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure services in Dependency Injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services into the DI container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Database Context
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });

            // Enable sensitive data logging only in development
            // options.EnableSensitiveDataLogging();
            // options.EnableDetailedErrors();
        });

        // Register Authentication Services
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
