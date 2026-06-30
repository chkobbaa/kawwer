using System.Net.Http.Json;
using System.Text.Json;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.Services;

/// <summary>Typed client over the Kawwer REST API. Unwraps the standard response envelope.</summary>
public sealed class KawwerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public KawwerApiClient(HttpClient http) => _http = http;

    // ----- Auth -----
    public Task<AuthResponse> RegisterAsync(object body, CancellationToken ct = default)
        => PostAsync<AuthResponse>("auth/register", body, ct);

    public Task<AuthResponse> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        => PostAsync<AuthResponse>("auth/login", new { usernameOrEmail, password }, ct);

    public Task LogoutAsync(string refreshToken, CancellationToken ct = default)
        => PostAsync("auth/logout", new { refreshToken }, ct);

    // ----- Users -----
    public Task<UserDto> GetMeAsync(CancellationToken ct = default) => GetAsync<UserDto>("users/me", ct);

    public Task<UserDto> UpdateProfileAsync(object body, CancellationToken ct = default)
        => PutAsync<UserDto>("users/me", body, ct);

    public Task<PlayerStatisticsDto> GetMyStatisticsAsync(CancellationToken ct = default)
        => GetAsync<PlayerStatisticsDto>("users/me/statistics", ct);

    public Task UpdateDeviceTokenAsync(string? token, CancellationToken ct = default)
        => PutAsync("users/me/device-token", new { deviceToken = token }, ct);

    // ----- Friends -----
    public Task<List<FriendDto>> GetFriendsAsync(CancellationToken ct = default) => GetAsync<List<FriendDto>>("friends", ct);

    public Task<List<FriendRequestDto>> GetFriendRequestsAsync(CancellationToken ct = default)
        => GetAsync<List<FriendRequestDto>>("friends/requests", ct);

    public Task SendFriendRequestAsync(Guid targetUserId, CancellationToken ct = default)
        => PostAsync("friends/requests", new { targetUserId }, ct);

    public Task AcceptFriendRequestAsync(Guid friendshipId, CancellationToken ct = default)
        => PostAsync($"friends/requests/{friendshipId}/accept", null, ct);

    public Task RejectFriendRequestAsync(Guid friendshipId, CancellationToken ct = default)
        => PostAsync($"friends/requests/{friendshipId}/reject", null, ct);

    public Task RemoveFriendAsync(Guid friendUserId, CancellationToken ct = default)
        => DeleteAsync($"friends/{friendUserId}", ct);

    public Task<List<UserSummaryDto>> SearchUsersAsync(string term, CancellationToken ct = default)
        => GetAsync<List<UserSummaryDto>>($"friends/search?term={Uri.EscapeDataString(term)}", ct);

    // ----- Groups -----
    public Task<List<GroupDto>> GetGroupsAsync(CancellationToken ct = default) => GetAsync<List<GroupDto>>("groups", ct);

    public Task<Guid> CreateGroupAsync(string name, string? description, CancellationToken ct = default)
        => PostAsync<Guid>("groups", new { name, description }, ct);

    public Task DeleteGroupAsync(Guid id, CancellationToken ct = default) => DeleteAsync($"groups/{id}", ct);

    public Task AddGroupMemberAsync(Guid groupId, Guid userId, CancellationToken ct = default)
        => PostAsync($"groups/{groupId}/members", new { userId }, ct);

    // ----- Football fields -----
    public Task<PagedResult<FootballFieldDto>> SearchFieldsAsync(string? search, CancellationToken ct = default)
        => GetAsync<PagedResult<FootballFieldDto>>($"football-fields?search={Uri.EscapeDataString(search ?? string.Empty)}", ct);

    public Task<Guid> CreateFieldAsync(object body, CancellationToken ct = default)
        => PostAsync<Guid>("football-fields", body, ct);

    // ----- Matches -----
    public Task<Guid> CreateMatchAsync(object body, CancellationToken ct = default) => PostAsync<Guid>("matches", body, ct);

    public Task<MatchDto> GetMatchAsync(Guid id, CancellationToken ct = default) => GetAsync<MatchDto>($"matches/{id}", ct);

    public Task<List<MatchParticipantDto>> GetParticipantsAsync(Guid id, CancellationToken ct = default)
        => GetAsync<List<MatchParticipantDto>>($"matches/{id}/participants", ct);

    public Task<List<OrganizerDashboardItemDto>> GetDashboardAsync(CancellationToken ct = default)
        => GetAsync<List<OrganizerDashboardItemDto>>("matches/dashboard", ct);

    public Task<List<MatchDto>> GetUpcomingAsync(CancellationToken ct = default)
        => GetAsync<List<MatchDto>>("matches/upcoming", ct);

    public Task RespondAsync(Guid matchId, bool accept, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/respond", new { accept }, ct);

    public Task LeaveAsync(Guid matchId, CancellationToken ct = default) => PostAsync($"matches/{matchId}/leave", null, ct);

    public Task CancelAsync(Guid matchId, CancellationToken ct = default) => PostAsync($"matches/{matchId}/cancel", null, ct);

    public Task FinishAsync(Guid matchId, CancellationToken ct = default) => PostAsync($"matches/{matchId}/finish", null, ct);

    public Task StartLiveAsync(Guid matchId, CancellationToken ct = default) => PostAsync($"matches/{matchId}/live/start", null, ct);

    public Task InviteAsync(Guid matchId, IEnumerable<Guid> userIds, IEnumerable<Guid> groupIds, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/invitations", new { userIds, groupIds }, ct);

    // ----- Payments -----
    public Task<PaymentSummaryDto> GetPaymentSummaryAsync(Guid matchId, CancellationToken ct = default)
        => GetAsync<PaymentSummaryDto>($"matches/{matchId}/payments", ct);

    public Task StartCollectionAsync(Guid matchId, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/payments/start", null, ct);

    public Task MarkPaidAsync(Guid matchId, Guid userId, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/payments/mark-paid", new { userId }, ct);

    public Task RecordPaymentAsync(Guid matchId, Guid userId, decimal amount, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/payments/record", new { userId, amount }, ct);

    public Task FinishCollectionAsync(Guid matchId, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/payments/finish", null, ct);

    // ----- Chat -----
    public Task<PagedResult<ChatMessageDto>> GetMessagesAsync(Guid matchId, int page = 1, int pageSize = 50, CancellationToken ct = default)
        => GetAsync<PagedResult<ChatMessageDto>>($"matches/{matchId}/chat/messages?page={page}&pageSize={pageSize}", ct);

    public Task<ChatMessageDto> SendMessageAsync(Guid matchId, string content, CancellationToken ct = default)
        => PostAsync<ChatMessageDto>($"matches/{matchId}/chat/messages", new { content }, ct);

    // ----- Notifications -----
    public Task<PagedResult<NotificationDto>> GetNotificationsAsync(int page = 1, int pageSize = 30, CancellationToken ct = default)
        => GetAsync<PagedResult<NotificationDto>>($"notifications?page={page}&pageSize={pageSize}", ct);

    public Task<int> GetUnreadCountAsync(CancellationToken ct = default) => GetAsync<int>("notifications/unread-count", ct);

    public Task MarkNotificationReadAsync(Guid id, CancellationToken ct = default) => PostAsync($"notifications/{id}/read", null, ct);

    public Task MarkAllReadAsync(CancellationToken ct = default) => PostAsync("notifications/read-all", null, ct);

    // ----- Public matches -----
    public Task<PagedResult<DiscoverMatchDto>> DiscoverAsync(double? lat, double? lng, double? radiusKm, CancellationToken ct = default)
    {
        var query = "public-matches/discover?page=1&pageSize=50";
        if (lat.HasValue && lng.HasValue)
        {
            query += $"&latitude={lat.Value}&longitude={lng.Value}";
            if (radiusKm.HasValue)
            {
                query += $"&radiusKm={radiusKm.Value}";
            }
        }

        return GetAsync<PagedResult<DiscoverMatchDto>>(query, ct);
    }

    public Task<bool> JoinPublicMatchAsync(Guid matchId, CancellationToken ct = default)
        => PostAsync<JoinResult>($"public-matches/{matchId}/join", null, ct).ContinueWith(t => t.Result.Accepted, ct);

    private sealed class JoinResult { public bool Accepted { get; set; } }

    // ----- HTTP plumbing -----
    private async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var response = await _http.GetAsync(path, ct);
        return await UnwrapAsync<T>(response, ct);
    }

    private async Task<T> PostAsync<T>(string path, object? body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(path, body ?? new { }, JsonOptions, ct);
        return await UnwrapAsync<T>(response, ct);
    }

    private async Task PostAsync(string path, object? body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(path, body ?? new { }, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task<T> PutAsync<T>(string path, object? body, CancellationToken ct)
    {
        using var response = await _http.PutAsJsonAsync(path, body ?? new { }, JsonOptions, ct);
        return await UnwrapAsync<T>(response, ct);
    }

    private async Task PutAsync(string path, object? body, CancellationToken ct)
    {
        using var response = await _http.PutAsJsonAsync(path, body ?? new { }, JsonOptions, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private async Task DeleteAsync(string path, CancellationToken ct)
    {
        using var response = await _http.DeleteAsync(path, ct);
        await EnsureSuccessAsync(response, ct);
    }

    private static async Task<T> UnwrapAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await ReadEnvelopeAsync<T>(response, ct);
        if (payload is null || !payload.Success || payload.Data is null)
        {
            throw new ApiException(payload?.Message ?? $"Request failed ({(int)response.StatusCode}).");
        }

        return payload.Data;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await ReadEnvelopeAsync<object>(response, ct);
        if (payload is null || !payload.Success)
        {
            throw new ApiException(payload?.Message ?? $"Request failed ({(int)response.StatusCode}).");
        }
    }

    private static async Task<ApiResponse<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
        }
        catch
        {
            throw new ApiException($"Request failed ({(int)response.StatusCode}).");
        }
    }
}
