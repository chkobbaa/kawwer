namespace Kawwer.Application.Common.Interfaces;

/// <summary>Sends a push notification to a single device (Firebase Cloud Messaging).</summary>
public interface IPushNotificationSender
{
    Task SendAsync(string deviceToken, string title, string body, IReadOnlyDictionary<string, string>? data = null, CancellationToken cancellationToken = default);
}
