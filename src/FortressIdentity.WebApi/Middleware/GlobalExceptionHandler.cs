using FluentValidation;
using FortressIdentity.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FortressIdentity.WebApi.Middleware;

/// <summary>
/// Global exception handler for the application.
/// Catches exceptions and converts them to appropriate HTTP responses.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        var problemDetails = exception switch
        {
            ValidationException validationException => HandleValidationException(validationException),
            InvalidEntityException invalidEntityException => HandleInvalidEntityException(invalidEntityException),
            DomainException domainException => HandleDomainException(domainException),
            _ => HandleUnknownException(exception)
        };

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }),
            cancellationToken);

        return true;
    }

    /// <summary>
    /// Handles FluentValidation exceptions.
    /// Returns 400 Bad Request with detailed validation errors.
    /// </summary>
    private static ProblemDetails HandleValidationException(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Extensions =
            {
                ["errors"] = errors
            }
        };
    }

    /// <summary>
    /// Handles domain-specific exceptions (e.g., business rule violations).
    /// Returns 409 Conflict for duplicate resources, 400 Bad Request otherwise.
    /// </summary>
    private static ProblemDetails HandleDomainException(DomainException exception)
    {
        // Check if it's a duplicate email error
        var isDuplicateError = exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase);

        return new ProblemDetails
        {
            Status = isDuplicateError ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
            Type = isDuplicateError
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                : "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = isDuplicateError ? "Conflict" : "Domain Error",
            Detail = exception.Message
        };
    }

    /// <summary>
    /// Handles invalid entity exceptions from the domain layer.
    /// Returns 400 Bad Request.
    /// </summary>
    private static ProblemDetails HandleInvalidEntityException(InvalidEntityException exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Invalid Entity",
            Detail = exception.Message
        };
    }

    /// <summary>
    /// Handles unexpected exceptions.
    /// Returns 500 Internal Server Error without exposing sensitive details.
    /// </summary>
    private static ProblemDetails HandleUnknownException(Exception exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please try again later."
        };
    }
}
