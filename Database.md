# Database — Overview

**Status:** Implemented · **Owner:** Kawwer Team

Kawwer stores its data in **PostgreSQL** via **Entity Framework Core**, with migrations owned by
`Kawwer.Infrastructure`. All primary keys are UUIDs and all timestamps are UTC.

Tables: `users`, `refresh_tokens`, `friendships`, `groups`, `group_members`, `football_fields`,
`matches`, `match_participants`, `payment_records`, `notifications`, `chat_messages`, `ratings`.

A key modelling decision (D-003/D-004/D-012): a player's whole lifecycle for a match — invitation,
response, waiting-list position, attendance, payment state, location sharing and rating flags —
lives on a single `match_participants` row. The immutable cash ledger is the exception, in
`payment_records`.

**→ Full schema (every field, type, length, index and the entity-relationship diagram):
[`docs/Database.md`](docs/Database.md).**
