using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Attaches the access token to every request. On a 401 it transparently refreshes the token
/// once (using the refresh token) and retries.
/// </summary>
public sealed class AuthHeaderHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Serializes token refreshes across ALL in-flight requests. The API rotates (revokes) the
    // refresh token on every use, so concurrent refreshes would kill the session.
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private readonly SessionState _session;

    public AuthHeaderHandler(SessionState session) => _session = session;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // At cold start the app routes to the main tabs immediately; make sure the persisted
        // tokens have finished loading before the first authenticated request goes out.
        await _session.EnsureLoadedAsync();

        var tokenUsed = _session.AccessToken;
        Attach(request);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrEmpty(_session.RefreshToken))
        {
            return response;
        }

        bool refreshed;
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Another request may have already refreshed the token while we waited.
            refreshed = (!string.IsNullOrEmpty(_session.AccessToken) && _session.AccessToken != tokenUsed)
                        || await TryRefreshAsync(cancellationToken);
        }
        finally
        {
            RefreshLock.Release();
        }

        if (!refreshed)
        {
            return response;
        }

        response.Dispose();
        var retry = await CloneAsync(request);
        Attach(retry);
        return await base.SendAsync(retry, cancellationToken);
    }

    private void Attach(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }
    }

    private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(
                $"{AppConfig.ApiBaseUrl}/auth/refresh",
                new { refreshToken = _session.RefreshToken },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // The refresh failed. The user is NEVER logged out automatically: only an
                // explicit logout clears the session. We keep the tokens so a later refresh
                // attempt (or the next app start) can recover the session.
                return false;
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions, cancellationToken);
            if (payload?.Data is null)
            {
                return false;
            }

            await _session.UpdateTokensAsync(payload.Data.AccessToken, payload.Data.RefreshToken);
            return true;
        }
        catch
        {
            // Transient/network failure: keep the session so a later request can retry.
            return false;
        }
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
