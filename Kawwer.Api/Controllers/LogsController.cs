using System.Security.Cryptography;
using System.Text;
using Kawwer.Api.Logging;
using Kawwer.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

/// <summary>
/// A minimal, hashed-password-gated window into recent server logs, readable from the phone or a
/// PC via <c>/logs</c>. No JWT: the caller proves knowledge of a shared password, which is compared
/// against a SHA-256 hash from configuration (<c>Logs:PasswordHash</c>) — the plaintext is never
/// stored or logged. This is intentionally simple; it is not a replacement for a full log platform.
/// </summary>
[AllowAnonymous]
[Route("api/v1/logs")]
public sealed class LogsController : ApiControllerBase
{
    private readonly InMemoryLogStore _store;
    private readonly IConfiguration _configuration;

    public LogsController(InMemoryLogStore store, IConfiguration configuration)
    {
        _store = store;
        _configuration = configuration;
    }

    public sealed record LogQueryRequest(string Password, string? MinLevel, int? Limit, long? AfterSequence);

    /// <summary>Verifies the password only, so the viewer can gate its UI before streaming logs.</summary>
    [HttpPost("auth")]
    public IActionResult Authenticate([FromBody] LogAuthRequest request)
    {
        if (!IsPasswordValid(request.Password))
        {
            return Unauthorized(ApiResponse.Fail("Incorrect password."));
        }

        return Ok(new { authenticated = true });
    }

    public sealed record LogAuthRequest(string Password);

    /// <summary>Returns recent log entries after verifying the password.</summary>
    [HttpPost]
    public IActionResult Query([FromBody] LogQueryRequest request)
    {
        if (!IsPasswordValid(request.Password))
        {
            return Unauthorized(ApiResponse.Fail("Incorrect password."));
        }

        var entries = _store.GetRecent(
            limit: request.Limit is > 0 and <= InMemoryLogStore.Capacity ? request.Limit.Value : 300,
            minLevel: request.MinLevel,
            afterSequence: request.AfterSequence ?? 0);

        return Ok(entries);
    }

    private bool IsPasswordValid(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return false;
        }

        var expectedHash = _configuration["Logs:PasswordHash"];
        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        var actualHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

        // Constant-time comparison so a timing side-channel can't reveal the hash byte by byte.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(actualHash),
            Encoding.ASCII.GetBytes(expectedHash.Trim().ToUpperInvariant()));
    }
}
