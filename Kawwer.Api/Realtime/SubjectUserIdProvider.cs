using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Kawwer.Api.Realtime;

/// <summary>
/// Maps a SignalR connection to its user id so <c>Clients.User(userId)</c> can target every
/// connection a single account has open. The access token carries the user id in the <c>sub</c>
/// claim, which JwtBearer surfaces as <see cref="ClaimTypes.NameIdentifier"/>; we read that first
/// and fall back to the raw <c>sub</c> claim, mirroring <c>ApiControllerBase</c>.
/// </summary>
public sealed class SubjectUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? connection.User?.FindFirstValue("sub");
}
