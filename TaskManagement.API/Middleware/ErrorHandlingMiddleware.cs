// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Exceptions;

namespace TaskManagement.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.Log(
                ex is ValidationException ? LogLevel.Warning : LogLevel.Error,
                ex,
                "An exception occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            NotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
            InvalidStatusTransitionException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception switch
            {
                ValidationException => "One or more validation errors occurred.",
                NotFoundException => "The requested resource was not found.",
                UnauthorizedException => "You are not authorized to perform this action.",
                InvalidStatusTransitionException => "Invalid status transition.",
                _ => "An unexpected error occurred."
            },
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public ErrorDetails Details { get; set; } = null!;
}

public class ErrorDetails
{
    public string Type { get; set; } = string.Empty;
}

public class ValidationErrorDetails : ErrorDetails
{
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
} 