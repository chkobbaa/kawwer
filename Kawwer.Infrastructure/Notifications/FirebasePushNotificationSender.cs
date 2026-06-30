using System.Net.Http.Headers;
using System.Net.Http.Json;
using Kawwer.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kawwer.Infrastructure.Notifications;

/// <summary>
/// Sends push notifications through Firebase Cloud Messaging. When no server key is configured
/// (e.g. local development) it logs and returns without throwing, so the calling use case is
/// never blocked by missing push credentials.
/// </summary>
public sealed class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebasePushNotificationSender> _logger;

    public FirebasePushNotificationSender(
        HttpClient httpClient,
        IOptions<FirebaseOptions> options,
        ILogger<FirebasePushNotificationSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string deviceToken,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogInformation("FCM not configured; skipping push to {DeviceToken}: {Title}", deviceToken, title);
            return;
        }

        var payload = new
        {
            to = deviceToken,
            notification = new { title, body },
            data = data ?? new Dictionary<string, string>()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.TryAddWithoutValidation("Authorization", $"key={_options.ServerKey}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("FCM push failed with status {Status} for {Title}", response.StatusCode, title);
        }
    }
}
