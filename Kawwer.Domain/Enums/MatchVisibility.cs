namespace Kawwer.Domain.Enums;

public enum MatchVisibility
{
    /// <summary>Invitations only: hidden from Discover, only invited players can join.</summary>
    Private = 1,

    /// <summary>Everyone: visible on Discover, anyone can request to join.</summary>
    Public = 2,

    /// <summary>Only friends: visible on Discover to the organizer's friends, who can request to join.</summary>
    FriendsOnly = 3
}
