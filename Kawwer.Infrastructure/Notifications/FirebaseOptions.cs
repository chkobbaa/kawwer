namespace Kawwer.Infrastructure.Notifications;

/// <summary>Firebase Cloud Messaging settings bound from configuration (section "Firebase").</summary>
public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    /// <summary>FCM server key. When empty, push notifications are skipped (in-app notifications still persist).</summary>
    public string ServerKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = "https://fcm.googleapis.com/fcm/send";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ServerKey);
}
