namespace Kawwer.Infrastructure.Notifications;

/// <summary>Firebase Cloud Messaging settings bound from configuration (section "Firebase").</summary>
public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    /// <summary>
    /// Path to the Firebase service-account JSON file (FCM HTTP v1 API).
    /// Download it from Firebase console > Project settings > Service accounts.
    /// When empty, push notifications are skipped (in-app notifications still persist).
    /// </summary>
    public string ServiceAccountJsonPath { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ServiceAccountJsonPath);
}
