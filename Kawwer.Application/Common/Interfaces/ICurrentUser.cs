namespace Kawwer.Application.Common.Interfaces;

/// <summary>Exposes the identity of the authenticated caller for the current request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }

    /// <summary>Returns the authenticated user id or throws if the request is anonymous.</summary>
    Guid RequireUserId();
}
