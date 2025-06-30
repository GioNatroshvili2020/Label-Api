using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using label_api.Exceptions;

namespace label_api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    protected readonly ILogger<GlobalExceptionHandler> _logger;
    protected readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public virtual async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is LabelApiException labelApiException && labelApiException.StatusCode < 500)
        {
            // Log warning for client errors (4xx)
            _logger.LogWarning(exception,
                "Label API Exception occurred. RequestPath: {RequestPath}, RequestMethod: {RequestMethod}, TraceId: {TraceId}, StatusCode: {StatusCode}",
                httpContext.Request.Path,
                httpContext.Request.Method,
                httpContext.TraceIdentifier,
                labelApiException.StatusCode);
        }
        else
        {
            // Log error for server errors (5xx) and unexpected exceptions
            _logger.LogError(exception,
                "Unhandled exception occurred. RequestPath: {RequestPath}, RequestMethod: {RequestMethod}, TraceId: {TraceId}",
                httpContext.Request.Path,
                httpContext.Request.Method,
                httpContext.TraceIdentifier);
        }

        var problemDetails = MapExceptionToProblemDetails(exception);
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    protected virtual ProblemDetails MapExceptionToProblemDetails(Exception exception)
    {
        return exception switch
        {
            // Handle our custom Label API exceptions
            LabelApiException labelApiEx => CreateProblemDetails(
                labelApiEx.StatusCode,
                labelApiEx),        
            // Default case for unexpected exceptions
            _ => CreateProblemDetails(500, "Internal Server Error", "An unexpected error occurred")
        };
    }

    protected virtual ProblemDetails CreateProblemDetails(int status, Exception exception)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = ((HttpStatusCode)status).ToString(),
            Detail = exception.Message
        };
    }

    protected virtual ProblemDetails CreateProblemDetails(int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
    }
} 