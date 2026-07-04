using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Kawwer.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kawwer.Infrastructure.Notifications;

/// <summary>
/// Sends push notifications through Firebase Cloud Messaging (HTTP v1 API, via the Firebase
/// Admin SDK). When no service-account file is configured (e.g. local development) it logs and
/// returns without throwing, so the calling use case is never blocked by missing credentials.
/// </summary>
public sealed class FirebasePushNotificationSender : IPushNotificationSender
{
    /// <summary>Must match the notification channel created by the mobile app.</summary>
    private const string AndroidChannelId = "kawwer_default";

    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebasePushNotificationSender> _logger;
    private readonly object _initLock = new();
    private bool _initAttempted;

    public FirebasePushNotificationSender(
        IOptions<FirebaseOptions> options,
        ILogger<FirebasePushNotificationSender> logger)
    {
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
        if (!TryEnsureInitialized())
        {
            _logger.LogInformation("FCM not configured; skipping push: {Title}", title);
            return;
        }

        try
        {
            // Data-only message: the Android app builds the notification itself in ALL app
            // states (foreground, background, killed). This is what enables action buttons
            // (Accept/Decline) and tap-to-open deep links into the right screen.
            var payload = data?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>();
            payload["title"] = title;
            payload["body"] = body;
            payload["channelId"] = AndroidChannelId;

            var message = new Message
            {
                Token = deviceToken,
                Data = payload,
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                // iOS needs an APNs alert to show notifications when the app is backgrounded.
                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string> { ["apns-priority"] = "10" },
                    Aps = new Aps
                    {
                        Alert = new ApsAlert { Title = title, Body = body },
                        Sound = "default"
                    }
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // An unreachable device or stale token must never break the calling use case.
            _logger.LogWarning(ex, "FCM push failed for {Title}", title);
        }
    }

    private bool TryEnsureInitialized()
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return true;
        }

        if (_initAttempted || !_options.IsConfigured)
        {
            return FirebaseApp.DefaultInstance is not null;
        }

        lock (_initLock)
        {
            if (_initAttempted || FirebaseApp.DefaultInstance is not null)
            {
                return FirebaseApp.DefaultInstance is not null;
            }

            _initAttempted = true;
            try
            {
                if (!File.Exists(_options.ServiceAccountJsonPath))
                {
                    _logger.LogWarning("Firebase service account file not found: {Path}", _options.ServiceAccountJsonPath);
                    return false;
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(_options.ServiceAccountJsonPath)
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK.");
                return false;
            }
        }
    }
}
