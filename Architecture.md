# Architecture — Overview

**Status:** Approved · **Owner:** Kawwer Team

Kawwer is a .NET 10 system built with **Clean Architecture**, **lightweight DDD**, **CQRS** (no
event sourcing), the **repository pattern** and **dependency injection**. A single .NET MAUI
mobile app talks only to an ASP.NET Core Web API; the API delegates to an application layer of
commands and queries over a PostgreSQL-backed domain.

```
Kawwer.Mobile → Kawwer.Api → Kawwer.Application → Kawwer.Domain
                              Kawwer.Infrastructure ↑ (implements Application interfaces)
                              Kawwer.Contracts (shared DTOs)
```

- **Domain** holds entities, enums and business rules — no EF, HTTP or framework code.
- **Application** holds use cases, validators and interfaces — no SQL or controllers.
- **Infrastructure** implements persistence (EF Core), auth (JWT/BCrypt), push (FCM) and jobs.
- **Api** wires everything up: controllers, middleware, SignalR hubs and Swagger; no business logic.
- Dependency rules are enforced by architecture tests in `Kawwer.Tests`.

**→ Full design, layer responsibilities, dependency rules, security, logging and acceptance
criteria: [`docs/Architecture.md`](docs/Architecture.md).** Key trade-offs are recorded as ADRs in
[`docs/Decisions.md`](docs/Decisions.md).
