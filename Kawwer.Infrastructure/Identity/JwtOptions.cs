namespace Kawwer.Infrastructure.Identity;

/// <summary>JWT settings bound from configuration (section "Jwt").</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Kawwer";
    public string Audience { get; set; } = "KawwerClients";

    /// <summary>Symmetric signing key. Must be at least 32 characters in production.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;

    /// <summary>
    /// Sessions never expire on their own: only an explicit logout (or server-side revocation)
    /// ends them. Each refresh also rotates in a brand-new token with a fresh window.
    /// </summary>
    public int RefreshTokenDays { get; set; } = 3650;
}
