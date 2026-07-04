using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Attaches the access token to every request. On a 401 it transparently refreshes the token
/// once (using the refresh token) and retries. If the server definitively rejects the refresh
/// token (it expired or was revoked), the session is cleared and the user is sent to the login
/// screen instead of the app hanging on a perpetual "loading" state.
/// </summary>
public sealed class AuthHeaderHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Serializes token refreshes across ALL in-flight requests. The API rotates (revokes) the
    // refresh token on every use, so concurrent refreshes would kill the session.
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    // Ensures a burst of concurrent 401s triggers exactly one logout + navigation.
    private static int _loggingOut;

    private readonly SessionState _session;

    public AuthHeaderHandler(SessionState session) => _session = session;

    private enum RefreshOutcome
    {
        /// <summary>A fresh access token was obtained.</summary>
        Refreshed,

        /// <summary>The server rejected the refresh token (expired/revoked). The session is dead.</summary>
        Rejected,

        /// <summary>A transient/network error; keep the session so a later attempt can recover.</summary>
        Transient
    }

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
        var outcome = RefreshOutcome.Transient;
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Another request may have already refreshed the token while we waited.
            if (!string.IsNullOrEmpty(_session.AccessToken) && _session.AccessToken != tokenUsed)
            {
                refreshed = true;
            }
            else
            {
                outcome = await TryRefreshAsync(cancellationToken);
                refreshed = outcome == RefreshOutcome.Refreshed;
            }
        }
        finally
        {
            RefreshLock.Release();
        }

        if (!refreshed)
        {
            // Only tear down the session when the server actually rejected the refresh token.
            // Transient failures keep the tokens so the next request (or app start) can recover.
            if (outcome == RefreshOutcome.Rejected)
            {
                ForceLogout();
            }

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

    private async Task<RefreshOutcome> TryRefreshAsync(CancellationToken cancellationToken)
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
                // A 4xx means the refresh token is no longer valid (expired/revoked): the session
                // is genuinely dead. A 5xx (or other) is transient, so we keep the session.
                return (int)response.StatusCode is >= 400 and < 500
                    ? RefreshOutcome.Rejected
                    : RefreshOutcome.Transient;
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions, cancellationToken);
            if (payload?.Data is null)
            {
                return RefreshOutcome.Transient;
            }

            await _session.UpdateTokensAsync(payload.Data.AccessToken, payload.Data.RefreshToken);
            return RefreshOutcome.Refreshed;
        }
        catch
        {
            // Network failure/timeout: keep the session so a later request can retry.
            return RefreshOutcome.Transient;
        }
    }

    /// <summary>Clears the dead session and routes to the login screen exactly once.</summary>
    private void ForceLogout()
    {
        if (Interlocked.Exchange(ref _loggingOut, 1) == 1)
        {
            return;
        }

        _session.Clear();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (Shell.Current is not null)
                {
                    await Shell.Current.GoToAsync("//login");
                }
            }
            catch
            {
                // The Shell may not be ready yet; the next app start will land on login anyway.
            }
            finally
            {
                Interlocked.Exchange(ref _loggingOut, 0);
            }
        });
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
