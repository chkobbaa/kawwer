using Kawwer.Application.Features.Auth;
using Kawwer.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/auth")]
public sealed class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.BirthDate,
            request.PreferredPosition,
            request.PreferredFoot);

        var result = await Dispatcher.SendAsync(command, cancellationToken);
        return CreatedResponse(result, "Account created successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new LoginCommand(request.UsernameOrEmail, request.Password), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new LogoutCommand(request.RefreshToken), cancellationToken);
        return OkMessage("Logged out.");
    }
}
