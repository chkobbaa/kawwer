# Kawwer — Requirements

**Status:** Approved · **Version:** 1.0 · **Last Updated:** 2026-06-30 · **Owner:** Kawwer Team

---

This document captures the Version 1 functional and non-functional requirements. Detailed
behavior for each feature lives in the matching document under `docs/`.

## Functional Requirements

### FR-1 Authentication & Accounts
- Users register with username, email and password; passwords are stored as BCrypt hashes.
- Users log in with username **or** email and receive a JWT access token plus a refresh token.
- Access tokens expire in 15 minutes; refresh tokens in 30 days and are revocable.
- Repeated failed logins lock the account temporarily.
- Users can view and edit their profile and set profile visibility (Public, FriendsOnly, Private).
- See `docs/Authentication.md`.

### FR-2 Friends
- Send, accept and reject friend requests; remove friends; block users; search users.
- A friendship is unique per directed pair and becomes mutual when accepted.
- See `docs/Friends.md`.

### FR-3 Groups
- Create, rename and delete private groups owned by one user.
- Add and remove members to speed up invitations.
- See `docs/Groups.md`.

### FR-4 Football Fields
- Create, update, search and view reusable fields with location, surface, capacity, duration,
  price, reservation fee and amenities.
- Price changes never affect previously created matches.
- See `docs/FootballField.md`.

### FR-5 Matches
- Create a match from a field (snapshotting price, fee and duration), edit it, change capacity,
  cancel and finish it.
- A match progresses through Draft → Published → Full → Playing → Finished, or Cancelled.
- See `docs/Match.md` and `docs/CreateMatch.md`.

### FR-6 Invitations, Responses & Waiting List
- Invite individual players and whole groups.
- Players mark seen/thinking and accept or decline.
- When full, accepted players go to a waiting list with strict first-accepted, first-promoted
  ordering; leaving promotes the next waiting player automatically.
- See `docs/Invitations.md` and `docs/WaitingList.md`.

### FR-7 Live Match
- Activate a match-day control center showing attendance, payment progress and optional live
  location; players can share/stop location and mark themselves arrived.
- See `docs/LiveMatch.md`.

### FR-8 Payments
- Cash only. Start collection, record full or partial payments, undo before completion, split the
  remaining balance among unpaid players, and finish only when the remaining amount is zero.
- The per-player share is the remaining amount divided by accepted players, rounded up; the
  organizer absorbs the rounding remainder.
- An immutable payment ledger records every transaction.
- See `docs/Payments.md`.

### FR-9 Match Chat
- One temporary chat per match. Accepted participants and the organizer can post; waiting-list
  players read only.
- Users may edit/delete their own messages within five minutes; system messages are automatic
  and immutable; the organizer may pin one message.
- See `docs/MatchChat.md`.

### FR-10 Public Matches
- Organizers may publish a match to the Discover feed with distance/date filters.
- Joining sends a request the organizer approves or rejects, unless auto-accept is enabled.
- See `docs/PublicMatches.md`.

### FR-11 Notifications
- Push notifications via FCM, each also stored as an in-app notification.
- Mark read, mark all read, delete; respect user preferences; critical notifications cannot be
  disabled.
- See `docs/Notifications.md`.

### FR-12 Ratings, Reputation & Statistics
- After a finished match, players submit anonymous 1–5 star organizer/player ratings.
- Reputation (0–100, starting at 100) and a reliability badge are derived automatically.
- Player and organizer statistics are computed from completed matches and cannot be edited.
- See `docs/RatingsAndReputation.md` and `docs/Statistics.md`.

### FR-13 Calendar
- Accepted players can add a match to their device calendar with reminders; entries update or are
  removed when the match changes or is cancelled.
- See `docs/Calendar.md`.

## Non-Functional Requirements

- **NFR-1 Architecture.** Clean Architecture with CQRS and the repository pattern; layered
  dependency rules enforced by architecture tests (`docs/Architecture.md`).
- **NFR-2 API.** Versioned REST under `/api/v1`, standard success/error envelopes, ProblemDetails
  for failures, pagination on list endpoints, OpenAPI/Swagger (`docs/API.md`).
- **NFR-3 Security.** HTTPS only; JWT-protected endpoints; passwords hashed; secrets never
  committed; sensitive data never logged.
- **NFR-4 Real-time.** SignalR powers live match, payments, waiting list and chat updates.
- **NFR-5 Persistence.** PostgreSQL via EF Core with migrations owned by Infrastructure.
- **NFR-6 Clients.** A single .NET MAUI app for Android and iOS that communicates only through the
  API.
- **NFR-7 Quality.** Unit tests (domain, application), integration tests (API) and architecture
  tests must pass before release.

## Out of Scope for Version 1

Online/card payments, SMS/email/WhatsApp delivery, cloud calendar sync, match templates and
recurring matches, tournaments and leagues, AI team balancing, and the advanced statistics
dashboard. See `Roadmap.md` (Version 2).
