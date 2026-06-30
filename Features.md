# Kawwer — Features

**Status:** Approved · **Version:** 1.0 · **Last Updated:** 2026-06-30 · **Owner:** Kawwer Team

---

A map of every Version 1 feature to its detailed design document and its place in the codebase.
For the delivery order see `Roadmap.md` and `docs/ImplementationRoadmap.md`.

## Version 1 Features

| Feature | What it does | Detailed doc |
|---------|--------------|--------------|
| **Authentication** | Register/login with JWT access + refresh tokens, lockout, profiles | `docs/Authentication.md` |
| **Friends** | Friend requests, accept/reject, remove, block, user search | `docs/Friends.md` |
| **Groups** | Private owner-scoped groups to speed up invitations | `docs/Groups.md` |
| **Football Fields** | Reusable fields with price/fee/amenities; price snapshotting | `docs/FootballField.md` |
| **Create Match** | Guided creation that auto-loads field info | `docs/CreateMatch.md` |
| **Match Management** | Edit, change capacity, cancel, finish; lifecycle states | `docs/Match.md` |
| **Invitations** | Invite players and groups; seen/thinking/accept/decline | `docs/Invitations.md` |
| **Waiting List** | First-accepted, first-promoted auto-promotion | `docs/WaitingList.md` |
| **Live Match** | Match-day control center: attendance, payments, live location | `docs/LiveMatch.md` |
| **Payments** | Cash collection, partial payments, split balance, ledger | `docs/Payments.md` |
| **Match Chat** | Temporary per-match chat with system + pinned messages | `docs/MatchChat.md` |
| **Public Matches** | Discover feed, join requests, organizer approval | `docs/PublicMatches.md` |
| **Notifications** | FCM push + persistent in-app notifications | `docs/Notifications.md` |
| **Ratings & Reputation** | Anonymous 1–5 star ratings, derived reliability badge | `docs/RatingsAndReputation.md` |
| **Statistics** | Player/organizer statistics computed from completed matches | `docs/Statistics.md` |
| **Calendar** | Add accepted matches to the device calendar with reminders | `docs/Calendar.md` |
| **GPS Location Sharing** | Optional live location, surfaced through Live Match | `docs/LiveMatch.md` |

## Cross-Cutting Concerns

| Concern | Where | Doc |
|---------|-------|-----|
| Architecture (Clean + CQRS) | All backend projects | `docs/Architecture.md` |
| REST API standards | `Kawwer.Api`, `Kawwer.Contracts` | `docs/API.md` |
| Data model | `Kawwer.Domain`, `Kawwer.Infrastructure` | `docs/Database.md` |
| Navigation & screens | `Kawwer.Mobile` | `docs/Navigation.md`, `docs/Screens.md` |
| Design system | `Kawwer.Mobile/Resources` | `docs/DesignSystem.md` |
| Key decisions (ADRs) | — | `docs/Decisions.md` |

## Where the Code Lives

```
Kawwer.Domain          Entities, enums, business rules (no infrastructure)
Kawwer.Application     CQRS commands/queries, validators, interfaces, mappings
Kawwer.Infrastructure  EF Core + PostgreSQL, JWT, BCrypt, FCM push, background jobs
Kawwer.Api             Controllers, auth, SignalR hubs, middleware, Swagger
Kawwer.Contracts       Request/response DTOs shared with clients
Kawwer.Mobile          .NET MAUI app (MVVM): Views, ViewModels, Services
Kawwer.Tests           Domain, application and architecture tests
```

## Version 2 (Planned)

Match templates, recurring matches, tournament mode, league mode, field ratings, AI team
balancing, smart player suggestions, and an advanced statistics dashboard. See `Roadmap.md`.
