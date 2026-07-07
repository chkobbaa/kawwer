using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.Services;

/// <summary>Typed client over the Kawwer REST API. Unwraps the standard response envelope.</summary>
public sealed class KawwerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
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

    /// <summary>Saves the first-run onboarding answers and returns the refreshed profile.</summary>
    public Task<UserDto> CompleteOnboardingAsync(object body, CancellationToken ct = default)
        => PutAsync<UserDto>("users/onboarding", body, ct);

    public Task<PlayerStatisticsDto> GetMyStatisticsAsync(CancellationToken ct = default)
        => GetAsync<PlayerStatisticsDto>("users/me/statistics", ct);

    public Task UpdateDeviceTokenAsync(string? token, CancellationToken ct = default)
        => PutAsync("users/me/device-token", new { deviceToken = token }, ct);

    /// <summary>Deletes (deactivates) the signed-in user's own account.</summary>
    public Task DeleteAccountAsync(CancellationToken ct = default) => DeleteAsync("users/me", ct);

    /// <summary>Uploads a new profile picture (multipart) and returns the refreshed profile.</summary>
    public async Task<UserDto> UploadProfilePhotoAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        using var response = await _http.PostAsync("users/me/photo", form, ct);
        return await UnwrapAsync<UserDto>(response, ct);
    }

    // ----- System -----
    public Task<AppVersionDto> GetAppVersionAsync(CancellationToken ct = default)
        => GetAsync<AppVersionDto>("system/version", ct);

    public Task<UserDto> GetUserAsync(Guid userId, CancellationToken ct = default)
        => GetAsync<UserDto>($"users/{userId}", ct);

    public Task<PlayerStatisticsDto> GetUserStatisticsAsync(Guid userId, CancellationToken ct = default)
        => GetAsync<PlayerStatisticsDto>($"users/{userId}/statistics", ct);

    /// <summary>Upcoming matches the user is organizing. Empty unless the viewer is a friend.</summary>
    public Task<List<MatchDto>> GetUserOrganizingAsync(Guid userId, CancellationToken ct = default)
        => GetAsync<List<MatchDto>>($"users/{userId}/organizing", ct);

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

    // ----- Teams -----
    public Task<List<TeamDto>> GetTeamsAsync(CancellationToken ct = default) => GetAsync<List<TeamDto>>("teams", ct);

    public Task<Guid> CreateTeamAsync(string name, string? description, CancellationToken ct = default)
        => PostAsync<Guid>("teams", new { name, description }, ct);

    public Task DeleteTeamAsync(Guid id, CancellationToken ct = default) => DeleteAsync($"teams/{id}", ct);

    public Task AddTeamMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default)
        => PostAsync($"teams/{teamId}/members", new { userId }, ct);

    // ----- Football fields -----
    public Task<PagedResult<FootballFieldDto>> SearchFieldsAsync(string? search, CancellationToken ct = default)
        => GetAsync<PagedResult<FootballFieldDto>>($"football-fields?search={Uri.EscapeDataString(search ?? string.Empty)}&pageSize=50", ct);

    public Task<FootballFieldDto> GetFieldAsync(Guid id, CancellationToken ct = default)
        => GetAsync<FootballFieldDto>($"football-fields/{id}", ct);

    public Task<Guid> CreateFieldAsync(object body, CancellationToken ct = default)
        => PostAsync<Guid>("football-fields", body, ct);

    public Task UpdateFieldAsync(Guid id, object body, CancellationToken ct = default)
        => PutAsync($"football-fields/{id}", body, ct);

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

    public Task<WaitingListPositionDto> GetWaitingListPositionAsync(Guid matchId, CancellationToken ct = default)
        => GetAsync<WaitingListPositionDto>($"matches/{matchId}/waiting-list", ct);

    // ----- Tactical lineup & guest players -----
    public Task<LineupDto> GetLineupAsync(Guid matchId, CancellationToken ct = default)
        => GetAsync<LineupDto>($"matches/{matchId}/lineup", ct);

    public Task<LineupDto> AutoBalanceLineupAsync(Guid matchId, CancellationToken ct = default)
        => PostAsync<LineupDto>($"matches/{matchId}/lineup/auto-balance", null, ct);

    public Task UpdateLineupSlotAsync(
        Guid matchId, LineupSlotKind kind, Guid targetId, TeamSide team, double positionX, double positionY, CancellationToken ct = default)
        => PutAsync($"matches/{matchId}/lineup/slot", new { kind, targetId, team, positionX, positionY }, ct);

    public Task<GuestPlayerDto> AddGuestAsync(Guid matchId, string name, int? skillLevel, CancellationToken ct = default)
        => PostAsync<GuestPlayerDto>($"matches/{matchId}/guests", new { name, skillLevel }, ct);

    public Task RemoveGuestAsync(Guid matchId, Guid guestId, CancellationToken ct = default)
        => DeleteAsync($"matches/{matchId}/guests/{guestId}", ct);

    // ----- Live match -----
    public Task UpdateAttendanceAsync(Guid matchId, Guid userId, AttendanceStatus attendance, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/live/attendance", new { userId, attendance }, ct);

    public Task ShareLocationAsync(Guid matchId, decimal latitude, decimal longitude, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/live/location", new { latitude, longitude }, ct);

    public Task StopSharingLocationAsync(Guid matchId, CancellationToken ct = default)
        => DeleteAsync($"matches/{matchId}/live/location", ct);

    public Task RequestLocationsAsync(Guid matchId, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/live/request-locations", null, ct);

    // ----- Ratings -----
    public Task SubmitRatingsAsync(Guid matchId, object body, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/ratings", body, ct);

    public Task InviteAsync(Guid matchId, IEnumerable<Guid> userIds, IEnumerable<Guid> teamIds, CancellationToken ct = default)
        => PostAsync($"matches/{matchId}/invitations", new { userIds, teamIds }, ct);

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

    public async Task<bool> JoinPublicMatchAsync(Guid matchId, CancellationToken ct = default)
    {
        var result = await PostAsync<JoinResult>($"public-matches/{matchId}/join", null, ct);
        return result.Accepted;
    }

    public Task ApproveJoinRequestAsync(Guid matchId, Guid userId, CancellationToken ct = default)
        => PostAsync($"public-matches/{matchId}/join-requests/{userId}/approve", null, ct);

    public Task RejectJoinRequestAsync(Guid matchId, Guid userId, CancellationToken ct = default)
        => PostAsync($"public-matches/{matchId}/join-requests/{userId}/reject", null, ct);

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
        var json = await response.Content.ReadAsStringAsync(ct);
        var payload = TryDeserialize<ApiResponse<T>>(json);
        if (payload is { Success: true, Data: not null })
        {
            return payload.Data;
        }

        throw new ApiException(ExtractErrorMessage(payload?.Message, json, (int)response.StatusCode));
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);
        var payload = TryDeserialize<ApiResponse<object>>(json);
        if (payload is { Success: true })
        {
            return;
        }

        throw new ApiException(ExtractErrorMessage(payload?.Message, json, (int)response.StatusCode));
    }

    private static T? TryDeserialize<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Builds a user-friendly error from the envelope, validation problem details, or RFC 9457 problem details.</summary>
    private static string ExtractErrorMessage(string? envelopeMessage, string json, int statusCode)
    {
        if (!string.IsNullOrWhiteSpace(envelopeMessage))
        {
            return envelopeMessage;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // ASP.NET validation problem details: { "errors": { "Field": ["message"] } }
            if (root.TryGetProperty("errors", out var errors))
            {
                if (errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in errors.EnumerateObject())
                    {
                        foreach (var item in property.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                return item.GetString()!;
                            }
                        }
                    }
                }
                else if (errors.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in errors.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            return item.GetString()!;
                        }
                    }
                }
            }

            // RFC 9457 problem details. "detail" may contain a stack trace in development, keep the first line.
            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
            {
                return detail.GetString()!.Split('\n')[0].Trim();
            }

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                return title.GetString()!;
            }
        }
        catch
        {
            // Not JSON; fall through to the generic message.
        }

        return $"Request failed ({statusCode}).";
    }
}
