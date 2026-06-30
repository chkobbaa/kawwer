# API — Overview

**Status:** Approved · **Owner:** Kawwer Team

Kawwer exposes a versioned REST API (JSON over HTTPS) under `/api/v1`. Successful responses use a
standard envelope (`success`, `data`, `message`); failures return either the standard error
envelope or RFC 9457 `ProblemDetails`. Protected endpoints require a Bearer JWT; list endpoints are
paginated. OpenAPI/Swagger documents every endpoint.

### Endpoint groups

| Area | Base route |
|------|------------|
| Auth | `POST /api/v1/auth/{register,login,refresh,logout}` |
| Users & Statistics | `/api/v1/users` (`/me`, `/me/statistics`, …) |
| Friends | `/api/v1/friends` |
| Groups | `/api/v1/groups` |
| Football Fields | `/api/v1/football-fields` |
| Matches & Live Match | `/api/v1/matches` (`/upcoming`, `/{id}`, `/{id}/live/*`, …) |
| Invitations & Waiting List | `/api/v1/matches/{id}/{invitations,respond,leave,waiting-list}` |
| Payments | `/api/v1/matches/{id}/payments/*` |
| Chat | `/api/v1/matches/{id}/chat/messages` |
| Ratings | `/api/v1/matches/{id}/ratings` |
| Public Matches | `/api/v1/public-matches` (`/discover`, `/{id}/join`, …) |
| Notifications | `/api/v1/notifications` |
| Real-time hub | `GET /hubs/match` (SignalR, JWT via `access_token`) |
| Health | `GET /health` |

**→ Full API standards (versioning, status codes, pagination, filtering, validation, idempotency,
uploads, rate limiting): [`docs/API.md`](docs/API.md).**
