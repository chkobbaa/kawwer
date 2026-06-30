# MatchParticipant Entity

## Purpose

A MatchParticipant represents the relationship between a User and a Match.

Every invited player has exactly one MatchParticipant record for each match.

This entity manages:

- Invitation status
- Waiting list
- Payments
- Attendance
- Ratings
- Live location
- Join and leave history

---

# Entity Definition

| Field | Type | Required | Description |
|--------|------|----------|-------------|
| Id | UUID | Yes | Primary Key |
| MatchId | UUID | Yes | References Matches |
| UserId | UUID | Yes | References Users |
| Status | Enum | Yes | See Participant Status |
| WaitingListPosition | Integer | No | Null unless waiting |
| InvitedAt | DateTime | Yes | |
| SeenAt | DateTime | No | |
| RespondedAt | DateTime | No | |
| JoinedAt | DateTime | No | |
| LeftAt | DateTime | No | |
| PaidAmount | Decimal | Yes | Default 0 |
| PaymentCompleted | Boolean | Yes | |
| Attendance | Enum | Yes | Unknown by default |
| SharedLocation | Boolean | Yes | Default False |
| Latitude | Decimal | No | |
| Longitude | Decimal | No | |
| RatedOrganizer | Boolean | Yes | Default False |
| RatedPlayers | Boolean | Yes | Default False |
| CreatedAt | DateTime | Yes | |

---

# Participant Status

- Invited
- Seen
- Thinking
- Accepted
- Declined
- WaitingList
- Removed
- Cancelled

---

# Attendance Status

- Unknown
- Present
- Late
- NoShow

---

# Lifecycle

Invited

↓

Seen

↓

Thinking

↓

Accepted

↓

Waiting List (if full)

↓

Promoted (if a spot opens)

↓

Present

↓

Rated

---

# Business Rules

## Invitation

Each invited player receives one MatchParticipant record.

Duplicate invitations are not allowed.

---

## Waiting List

If the match is already full, newly accepted players are placed on the waiting list.

Waiting list order is determined by acceptance time.

---

## Promotion

When an accepted player leaves the match, the first waiting player is automatically promoted.

All affected players receive a notification.

---

## Leaving

A player may leave freely until 48 hours before the match.

Within 48 hours:

- If the waiting list is not empty, leaving is allowed.
- Otherwise, the player receives a warning that leaving may result in an unfair match.

Leaving is still permitted.

---

## Payments

Payment is managed by the organizer.

Each participant records:

- Amount paid
- Payment completion status

---

## Attendance

Attendance is confirmed after the match starts.

Possible values:

- Present
- Late
- NoShow

Late cancellations and no-shows reduce reputation.

---

## Ratings

After a match finishes, each participant may:

- Rate the organizer
- Rate other participants

A participant can submit ratings only once.

---

## Live Location

When requested by the organizer, participants may choose to share their location.

Location sharing ends automatically when the match finishes.

---

# Future Extensions

This entity supports future features including:

- Tournament participation
- Player substitutions
- Match MVP
- Automatic reliability calculation
- Achievement system