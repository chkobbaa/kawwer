namespace Kawwer.Domain.Enums;

/// <summary>
/// The sport a match is played in. Football remains the default so existing matches (created
/// before multi-sport support) keep their meaning after migration.
/// </summary>
public enum SportType
{
    Football = 1,
    Basketball = 2,
    Tennis = 3
}
