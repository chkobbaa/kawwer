using System.Text.Json;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Contracts.Common;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Api.Middleware;

/// <summary>
/// Translates exceptions into the standard error envelope from docs/API.md and the right status code.
/// Unexpected errors fall through to RFC 9457 ProblemDetails with HTTP 500.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteEnvelopeAsync(context, StatusCodes.Status400BadRequest, "Validation failed.", ex.Errors);
        }
        catch (NotFoundException ex)
        {
            await WriteEnvelopeAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteEnvelopeAsync(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteEnvelopeAsync(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteEnvelopeAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            await WriteEnvelopeAsync(context, StatusCodes.Status401Unauthorized, "Authentication is required.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
            await WriteProblemAsync(context, ex);
        }
    }

    private static async Task WriteEnvelopeAsync(HttpContext context, int statusCode, string message, IReadOnlyList<string>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = ApiResponse.Fail(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = "https://datatracker.ietf.org/doc/html/rfc9457",
            title = "An unexpected error occurred.",
            status = StatusCodes.Status500InternalServerError,
            detail = _environment.IsDevelopment() ? ex.ToString() : "Please try again later."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
