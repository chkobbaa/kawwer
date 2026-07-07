namespace Kawwer.Mobile.Models;

// Client-side mirrors of the server enums. The API serialises enums as their integer values,
// so these must keep the same numeric values as the backend.

public enum PreferredPosition { Goalkeeper = 1, Defender = 2, Midfielder = 3, Forward = 4 }

public enum PreferredFoot { Left = 1, Right = 2, Both = 3 }

public enum ProfileVisibility { Public = 1, FriendsOnly = 2, Private = 3 }

public enum SurfaceType { ArtificialTurf = 1, NaturalGrass = 2, Concrete = 3 }

public enum MatchVisibility { Private = 1, Public = 2, FriendsOnly = 3 }

public enum MatchStatus { Draft = 1, Published = 2, Full = 3, Playing = 4, Finished = 5, Cancelled = 6 }

public enum ParticipantStatus
{
    Invited = 1, Seen = 2, Thinking = 3, Accepted = 4, Declined = 5, WaitingList = 6, Removed = 7, Cancelled = 8
}

public enum AttendanceStatus { Unknown = 1, Travelling = 2, Present = 3, Late = 4, NoShow = 5 }

public enum PaymentStatus { NotPaid = 1, PartiallyPaid = 2, Paid = 3 }

public enum NotificationCategory { Match = 1, Invitation = 2, Payment = 3, LiveMatch = 4, Friend = 5, Team = 6, WaitingList = 7 }

public enum MatchFormat { Pickup = 1, VsExternalTeam = 2, VsAppTeam = 3 }

public enum RatingType { Organizer = 1, Player = 2 }

public enum ReliabilityBadge { VeryReliable = 1, Reliable = 2, OccasionallyCancels = 3, OftenLate = 4, FrequentNoShow = 5 }
