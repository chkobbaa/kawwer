# Fix Pack — Onboarding, Home, Profile, Matches, Notifications, Logs

**Status:** Implemented · **Scope:** `Kawwer.Api`, `Kawwer.Application`, `Kawwer.Domain`,
`Kawwer.Contracts`, `Kawwer.Infrastructure`, `Kawwer.Mobile`

This change set resolves a batch of issues reported after a build-and-run pass, adds two new
capabilities (match rescheduling with a Notify/Call delivery mode, and a password-locked server
log viewer), and repairs a build break that stopped the Android app from compiling against the
current .NET 10 MAUI.

---

## TL;DR — root causes

Several reported errors were **not code bugs** but a **stale backend deployment**: the app had been
rebuilt with newer endpoints while `api.bahroun.com` still ran an older API. Verified locally
(Postgres + the API) that the current code is correct:

| Report | Cause | Status |
|---|---|---|
| Preferences submit → **405** | Onboarding endpoint missing on the deployed API. A `PUT` to any route the old API doesn't have returns **405** (not 404) under `[ApiController]`. Current code returns **200**. | Redeploy fixes it. App no longer dead-ends (fallback added). |
| Teams → **request failed 404** | `Groups`→`Teams` rename not deployed (`/api/v1/teams` returns 200 on current code; old `/groups` is gone). | Redeploy fixes it. |
| Create match → **"The InviteGroupIds field is required"** | Old contract on the deployed API. Current contract uses `invitedUserIds` / `invitedTeamIds`; the string `InviteGroupIds` no longer exists anywhere in the repo. | Redeploy fixes it. |

Everything else below is a real code change.

> **Action required:** redeploy `Kawwer.Api` after merging so the mobile app talks to a matching backend.

---

## Build break fixed (Android)

`CropPhotoPage.RenderCrop` used `GraphicsPlatform.CurrentService.CreateBitmapExportContext(...)`,
which **was removed in .NET 10 MAUI** (`Microsoft.Maui.Graphics` 10.x). Against the toolchain the
CI (`codemagic.yaml`) uses (SDK `10.0.109` → MAUI `10.0.20`) the mobile project **did not compile**.
Replaced with the current API:

```csharp
using var context = new PlatformBitmapExportService().CreateContext(OutputSize, OutputSize, 1f);
```

Also disambiguated `IImage` → `Microsoft.Maui.Graphics.IImage` (MAUI 10 added a colliding
`Microsoft.Maui.IImage`). The whole `net10.0-android` build now succeeds.

---

## 1. Onboarding is never a dead-end

If the dedicated onboarding endpoint is unavailable (e.g. the backend is briefly behind), the flow
now falls back to the standard profile update and marks onboarding complete on-device, so the user
always reaches the app instead of being stuck on a 405.

- `OnboardingViewModel.ContinueAsync` — try `PUT users/onboarding`, fall back to `PUT users/me`.
- `SessionState.MarkOnboardingCompleted()` — persists the local completion flag.

## 2. Home empty state actually renders

`CollectionView.EmptyView` collapses to zero height inside a `ScrollView` (a known MAUI quirk), so
the Home screen showed only the "Upcoming" / "You're organizing" titles. Replaced with explicit
`EmptyStateView`s driven by `HasUpcoming` / `HasDashboard` / `ShowUpcomingEmpty` / `ShowDashboardEmpty`.

## 3. Notification bell updates in real time

The bell only refreshed on manual pull. Now Home refreshes the unread badge on **every** appearance
(`RefreshUnreadCommand`) in addition to the live SignalR signal, so a notification that arrived while
another tab was open is reflected immediately on return.

## 4. Profile picture

- **Avatar URL scheme** — behind the Caddy reverse proxy, generated avatar URLs came out as
  `http://…`, which Android blocks as cleartext, so the image silently failed and the initials
  (rendered *beneath* the photo) showed through — looking like a "revert to default". Added
  `UseForwardedHeaders` (honouring `X-Forwarded-Proto`) so uploaded URLs are `https://…`.
- **Robust upload** — empty picks are rejected, a degenerate crop render falls back to the source
  image, and a null returned URL keeps the existing photo instead of clearing it. Errors are shown.
- Gallery selection is single-shot (`PickPhotoAsync`), returning immediately on one pick.

## 5. Stars above the avatar (premium polish)

`CurvedStarsDrawable` / `RatedAvatarView`: smaller stars (`0.088·d` vs `0.12·d`), a tighter gap so
they hug the avatar, and a subtle per-star **warp** (outward lean + vertical squash that grows with
distance from centre) so side stars read as a gentle curved crown.

## 6. Match creation screen

- `WHERE` → **Field**, `WHAT` → **Game** with a sport dropdown (**Football / Basketball / Tennis**).
- **Opponent** section moved **before Players**.
- Players tile: **`<total>` + "including you"** side by side, plus **`<total/2>` per Team.**
- Match format label `Pickup match` → **`Friendly - Local players`**.
- Cleaner grouped layout.

Backend: new `SportType` (`Football`/`Basketball`/`Tennis`) on `Match`, threaded through the create
command/contract/DTO and persisted (migration; existing matches default to Football).

## 7. Friend-request notifications are interactive

The persisted `Notification` now stores `Type` and `RelatedFriendshipId`, so the in-app list renders
**Accept / Decline** buttons on friend requests (wired to the friendship), matching match invitations.

## 8. Matches expire automatically (timezone-safe)

- New terminal status **`Expired`**. A `MatchLifecycleService` sweep (and a defensive check on
  responses) moves any still-open match to `Expired` once its **scheduled end** passes.
- Kickoff/end are computed in the **app time zone** (`IDateTimeProvider.AppTimeZone`, default
  `Africa/Tunis`) instead of naïvely as UTC — so a "20:00" match expires at 20:00 local, not 20:00 UTC.
- Responding to (declining/accepting) an expired match is rejected **before** any organizer
  notification is sent, and expiring a match purges its stale invitation notifications. Expired
  matches show a banner and hide all action buttons in the app.

## 9. Reschedule + Notify/Call delivery mode

- New `POST /api/v1/matches/{id}/reschedule` (`RescheduleMatchCommand`) moves a match and notifies
  **all** participants **and** the waiting list. The notification is flagged `Important` + typed
  `match_rescheduled`. Organizer UI added to the match details screen (date/time pickers).
- New on-device **Notify ↔ Call** toggle (Settings). In **Call** mode, an `Important` real-time
  event triggers a simulated **incoming-call screen for ~3s** (`CallSimulationService` +
  `IncomingCallPage`) — then the normal notification still lands. In **Notify** mode nothing changes.

## 10. Password-locked server logs (`/logs`)

- In-memory ring buffer (`InMemoryLogStore` + `InMemoryLoggerProvider`) captures recent server logs.
- `POST /api/v1/logs` (+ `/auth`) verify a **SHA-256-hashed** password (`Logs:PasswordHash`, constant-
  time compare; plaintext never stored) and return recent entries with level filtering.
- A responsive viewer page at **`/logs`** (phone or PC): password gate, level filter, auto-refresh.
- Dev default password is `kawwer-logs`. **Override in production** by setting `Logs__PasswordHash`
  to the SHA-256 hex of your chosen password.

---

## Tests & verification

- 70 domain/application tests pass, incl. new `MatchLifecycleTests` (timezone-aware expiry,
  reschedule, sport default).
- Verified end-to-end against a local Postgres + API: match create with sport, reschedule
  (important notification), background auto-expiry (`Published → Expired`), blocked responses to
  expired matches (no organizer ping), friend-request notification payload, and the `/logs` gate.
- `net10.0-android` build of `Kawwer.Mobile` succeeds against the CI toolchain.

## Migration

`AddSportTypeAndNotificationActions` adds `matches.Sport` (default `1` = Football) and
`notifications.Type` / `RelatedFriendshipId` / `Important`. Applied automatically on API startup
(`Database:AutoMigrate`).
