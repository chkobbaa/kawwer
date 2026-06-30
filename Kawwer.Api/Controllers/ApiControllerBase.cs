using System.Security.Claims;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

/// <summary>
/// Base for all API controllers. Exposes the dispatcher and the authenticated user's id, and
/// wraps results in the standard <see cref="ApiResponse{T}"/> envelope.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private IDispatcher? _dispatcher;

    protected IDispatcher Dispatcher =>
        _dispatcher ??= HttpContext.RequestServices.GetRequiredService<IDispatcher>();

    /// <summary>The authenticated user's id, parsed from the JWT subject claim.</summary>
    protected Guid CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("The request is not authenticated.");
        }
    }

    protected IActionResult Ok<T>(T data, string? message = null) => base.Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult OkMessage(string? message = null) => base.Ok(ApiResponse.Ok(message));

    protected IActionResult CreatedResponse<T>(T data, string? message = null)
        => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));
}
