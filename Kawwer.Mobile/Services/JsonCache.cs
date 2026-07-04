using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Tiny JSON-over-Preferences cache. Used to persist small DTOs (profile, home data) so a cold
/// start can paint instantly from the last known values while the network refresh runs in the
/// background. Enum handling matches <see cref="KawwerApiClient"/> so cached and fresh payloads
/// deserialize identically.
/// </summary>
public static class JsonCache
{
    /// <summary>Well-known cache keys, cleared together when the session ends.</summary>
    public static class Keys
    {
        public const string ProfileUser = "cache_profile_user";
        public const string ProfileStats = "cache_profile_stats";

        public static readonly string[] All = { ProfileUser, ProfileStats };
    }

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Save<T>(string key, T value)
    {
        try
        {
            Preferences.Default.Set(key, JsonSerializer.Serialize(value, Options));
        }
        catch
        {
            // Caching is best-effort; never let a serialization hiccup break a save.
        }
    }

    public static T? Load<T>(string key)
    {
        try
        {
            var json = Preferences.Default.Get(key, string.Empty);
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, Options);
        }
        catch
        {
            return default;
        }
    }

    public static void Remove(string key)
    {
        try
        {
            Preferences.Default.Remove(key);
        }
        catch
        {
            // Ignore.
        }
    }
}
