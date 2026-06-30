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
    private readonly SessionState _session;

    public AuthHeaderHandler(SessionState session) => _session = session;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Attach(request);
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrEmpty(_session.RefreshToken))
        {
            return response;
        }

        if (!await TryRefreshAsync(cancellationToken))
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
                return false;
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>(JsonOptions, cancellationToken);
            if (payload?.Data is null)
            {
                return false;
            }

            _session.UpdateTokens(payload.Data.AccessToken, payload.Data.RefreshToken);
            return true;
        }
        catch
        {
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
