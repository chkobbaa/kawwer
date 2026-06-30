# Match Entity

## Purpose

A Match represents a football game organized by one user (Organizer) at a specific football field.

A Match manages:

- Invitations
- Accepted players
- Waiting list
- Payments
- Chat
- Ratings
- Attendance
- Notifications

---

# Match Information

| Field | Type | Required | Description |
|--------|------|----------|-------------|
| Id | UUID | Yes | Primary Key |
| OrganizerId | UUID | Yes | References Users |
| FootballFieldId | UUID | Yes | References Football Fields |
| Title | String | Yes | Example: Friday Night Match |
| Description | String | No | Optional notes |
| Visibility | Enum | Yes | Private, Public |
| Status | Enum | Yes | Draft, Published, Full, Playing, Finished, Cancelled |
| MatchDate | Date | Yes | |
| StartTime | Time | Yes | |
| EndTime | Time | Yes | Calculated from field duration |
| MaxPlayers | Integer | Yes | 10, 12, 14, 16... |
| ReservationPaid | Decimal | Yes | Amount already paid |
| TotalFieldPrice | Decimal | Yes | Full field price |
| RemainingAmount | Decimal | No | Calculated |
| SharePerPlayer | Decimal | No | Calculated |
| CreatedAt | DateTime | Yes | |
| UpdatedAt | DateTime | Yes | |

---

# Match Rules

- A match has exactly one organizer.
- A match uses one football field.
- A match has one chat.
- A match can have many participants.
- A match can have a waiting list.
- A match can be public or private.

---

# Match Status Lifecycle

Draft

↓

Published

↓

Full

↓

Playing

↓

Finished

or

Cancelled

---

# Automatic Calculations

RemainingAmount =
TotalFieldPrice - ReservationPaid

SharePerPlayer =
Ceiling(RemainingAmount / NumberOfPlayersExcludingOrganizer)

The organizer keeps any remaining amount caused by rounding.

---

# Notifications

The system sends notifications when:

- Match published
- Player accepted
- Player declined
- Player joined waiting list
- Waiting list promotion
- Match cancelled
- Money collection started
- 24-hour reminder
- 3-hour reminder
- 30-minute reminder

---

# Future Extensions

The Match entity is designed to support future features including:

- Tournament mode
- League mode
- Match photos
- Match videos
- Match statistics
- AI team balancing