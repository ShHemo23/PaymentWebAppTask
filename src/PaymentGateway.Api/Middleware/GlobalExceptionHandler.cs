using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace PaymentGateway.Api.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        // If our helper didn't map the exception we fall back to 500.
        problemDetails ??= new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An internal server error occurred.",
            Detail = "An unexpected error occurred while processing your request. Please try again later.",
            Instance = httpContext.Request.Path
        };
        
        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new ValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    ))
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = httpContext.Request.Path
            },
            InvalidOperationException ioe => new ProblemDetails
            {
                Title = "A request validation error occurred.",
                Status = StatusCodes.Status400BadRequest,
                Detail = ioe.Message,
                Instance = httpContext.Request.Path
            },
            KeyNotFoundException knf => new ProblemDetails
            {
                Title = "The requested resource was not found.",
                Status = StatusCodes.Status404NotFound,
                Detail = knf.Message,
                Instance = httpContext.Request.Path
            },
            _ => new ProblemDetails
            {
                Title = "An unexpected server error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Please contact support if the problem persists.",
                Instance = httpContext.Request.Path
            }
        };
    }
}
