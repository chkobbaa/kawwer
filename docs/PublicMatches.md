# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Public Matches

## Purpose

Public Matches allow organizers to fill empty player spots by making selected matches discoverable to other Kawwer users.

Organizers remain in full control of who joins their matches.

---

# Visibility

A match may be:

- Private
- Public

Private matches are visible only to invited players.

Public matches appear in the Discover page.

---

# Discover

Players may browse public matches using filters.

Available filters:

- Distance
- Date
- Start Time
- Available Spots
- Football Field
- Indoor / Outdoor
- Surface Type
- Match Duration

Future filters:

- Skill Level
- Age Range
- Language

---

# Joining a Public Match

A player taps:

Join Match

↓

A join request is sent.

↓

Organizer receives notification.

↓

Organizer chooses:

Accept

or

Reject

---

# Organizer Approval

Version 1 requires organizer approval.

Automatic acceptance is optional.

If enabled, the organizer may choose:

- Accept everyone automatically.
- Accept only players with a Reliable or Very Reliable badge.

---

# Join Requests

For every request, the organizer sees:

- Username
- Reliability Badge
- Average Rating
- Matches Played
- Late Cancellation Count
- No-Show Count

The organizer may also open the player's profile before making a decision.

---

# Accepted Players

When approved:

- Player becomes MatchParticipant.
- Push notification is sent.
- Calendar entry is created.
- Player appears in participant list.

---

# Rejected Players

Rejected players receive a notification.

Rejected players may submit another request only if:

- The organizer changes the match settings.
- The organizer explicitly invites them.

---

# Match Full

Once the match reaches capacity:

- New requests are placed into the waiting list.
- Waiting list order follows WaitingList.md.

---

# Search Radius

Default search radius:

25 km

Users may change it:

- 5 km
- 10 km
- 25 km
- 50 km
- 100 km

---

# Business Rules

- Only public matches appear in Discover.
- Private match information remains hidden.
- Organizer always controls approval unless automatic acceptance is enabled.
- Public players cannot bypass the waiting list.

---

# Validation Rules

A player cannot:

- Join twice.
- Join after match cancellation.
- Join after match completion.

Blocked users cannot request to join private matches.

---

# Acceptance Criteria

- Public matches appear in Discover.
- Filters work correctly.
- Join requests notify organizers.
- Organizer can approve or reject requests.
- Accepted players become MatchParticipants.
- Full matches use the waiting list.