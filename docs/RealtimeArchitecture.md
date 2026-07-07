# Real-Time Architecture — Decision & Justification

**Status:** Implemented · **Owner:** Kawwer Team · **Feature:** Real-time updates + interactive push (Feature 6)

This document records the architecture decision required by Feature 6: *inspect the existing
system, compare the real-time options, choose the best one, justify it, and only then implement.*

---

## 1. What the system already had

Before this change, the pieces were half in place but disconnected:

| Layer | State before |
|-------|--------------|
| **Backend (ASP.NET)** | SignalR was already wired: `MatchHub` (per-match groups), `SignalRRealtimeNotifier : IRealtimeNotifier`, hub mapped at `/hubs/match`, JWT auth over the `access_token` query string. It was used for **match-scoped** events only: live match, payments, waiting list, chat. |
| **Backend notifications** | `NotificationService` fanned every event out to an in-app record + **data-only FCM** + Web Push (VAPID/PWA). The FCM payload already carried actionable metadata (`type`, `matchId`, `friendshipId`). |
| **Android** | Already rendered **interactive Accept/Decline** buttons from the data-only FCM message and executed them in the background via `NotificationActionReceiver` (no app open required), with deep-link fallback on tap. |
| **MAUI app** | **No real-time client at all.** Every screen relied on `OnAppearing` + a 30-second staleness check + pull-to-refresh. `AppConfig.HubBaseUrl` pointed at the hub, but nothing consumed it. |
| **PWA** | Push via service worker + VAPID; no live socket. |

So the gap was **not** "add a real-time technology from scratch." It was:

1. There was **no user-scoped** real-time channel — friend requests, invitations and profile
   changes never reached a *connected* client live; they only produced a push + an in-app record.
2. The **mobile app had no client** to consume any of it.

## 2. Options compared

| Option | Server scalability | Battery | Network efficiency | Mobile perf | PWA compat | Latency | Simplicity | Maintainability |
|--------|--------------------|---------|--------------------|-------------|------------|---------|------------|-----------------|
| **SignalR** (chosen) | Good; scales out with a Redis backplane (already the documented future) | One managed socket, idle-friendly; drops to nothing when backgrounded | High: negotiated WebSockets, tiny frames | First-class .NET client | **Yes** — official JS client, auto-fallback to SSE/long-polling | Sub-second | **Highest here** — infra + auth already exist | High — one abstraction (`IRealtimeNotifier`) already in the codebase |
| Raw WebSockets | Manual scale-out | Same as SignalR | High | Hand-rolled framing/reconnect | Yes, but you build it | Sub-second | Low — reimplements what SignalR gives | Low — bespoke protocol |
| Server-Sent Events | Good | One-way stream only | High for server→client | OK, needs polyfill patterns | Yes | Sub-second | Medium, but one-way | Medium; would need a second channel for client→server (join match) |
| Long polling | Poor (held connections/timers) | Bad — constant reconnects wake the radio | Wasteful | OK | Yes | Seconds | Medium | Medium |
| Client polling | Poor (multiplies load) | **Worst** — periodic wakeups | **Worst** — repeated fetches of unchanged data | OK | Yes | Tunable but always a trade-off vs. battery | High | Low (explicitly disallowed by the acceptance criteria) |
| FCM/Web Push **only** (no socket) | Excellent (offloaded) | Excellent when idle | Excellent | Good | Yes | 1–3 s, and **not guaranteed for foreground/live UI** | Already present | Good, but can't drive smooth in-app live updates |

## 3. Decision

**Use SignalR for in-app real-time synchronization, and keep FCM/Web Push for background delivery
and interactive notification actions — a hybrid, each doing what it is best at.**

- **SignalR** drives the *foreground* experience: while the app is open, screens update instantly
  from a single authenticated socket. No polling.
- **FCM data messages** drive the *background* experience: when the app is killed or backgrounded,
  the OS shows the notification and the **Accept/Decline** action buttons, which execute in the
  background or deep-link into the app.

### Why SignalR wins here

1. **It's already the sanctioned technology.** `docs/Architecture.md` lists SignalR as the V1
   real-time stack, and the hub, DI registration and JWT-over-query-string auth already exist.
   Choosing anything else would mean throwing away working, tested infrastructure and maintaining
   two paradigms.
2. **Transport negotiation gives us PWA + resilience for free.** SignalR uses WebSockets when
   available and transparently falls back to Server-Sent Events or long polling. The same server
   serves the native `Microsoft.AspNetCore.SignalR.Client` and the browser JS client.
3. **Battery and network.** One idle-friendly socket with automatic reconnect beats polling and
   beats hand-rolled sockets. When the app is backgrounded we lean on FCM instead of holding a
   socket open, so we don't drain the radio.
4. **Bidirectional with almost no code.** Clients call `JoinMatch`/`LeaveMatch` to scope match
   updates — trivial with SignalR, extra plumbing with SSE.
5. **Simplicity & maintainability.** The whole surface stays behind the existing
   `IRealtimeNotifier` abstraction; the Application layer never learns it's SignalR.

## 4. What was implemented

**Backend**

- `IRealtimeNotifier` gained one user-scoped method: `NotifyUserAsync(userId, RealtimeUserEvent)`.
- `SignalRRealtimeNotifier` implements it with `Clients.User(userId)`, and a `SubjectUserIdProvider`
  maps a connection to its JWT subject so those messages route correctly.
- `NotificationService` now emits a `UserEvent` for **every** in-app notification, as the *last*
  step before the caller commits — so friend requests, invitations, match status changes, player
  joins/leaves and waiting-list moves all become live automatically. Profile edits emit a `Profile`
  event for multi-device sync.

**Mobile**

- A new `RealtimeService` owns a single `HubConnection` (auth token provider + automatic reconnect),
  exposes plain C# events, and ref-counts match-group membership so overlapping screens don't
  unsubscribe each other. It starts on login/app-launch and stops on logout.
- View models subscribe on `OnAppearing` and unsubscribe on `OnDisappearing`, re-loading through the
  normal API when a relevant event arrives. No XAML changed.

## 5. Ordering & consistency note

The user-scoped signal is sent **after** the (slower) FCM/Web Push work and immediately **before**
the caller's `SaveChangesAsync`. Because a local commit is far faster than the network round trip a
client needs to react and re-fetch, the client reliably observes the just-committed state. The
signal is best effort; a missed one self-heals on the next screen load. For horizontal scale-out,
the documented next step (a Redis backplane, per `docs/Architecture.md`) lets `Clients.User` reach a
user's connections across multiple API hosts without any change to the Application layer.
