# Database Design

This document describes the entities and relationships used by the Kawwer application.

The purpose of this document is to define the data model before implementation begins. Every entity described here will eventually become a database table.

---

# Entity List

- Users
- Friends
- Groups
- Group Members
- Football Fields
- Matches
- Match Participants
- Payments
- Notifications
- Chats
- Messages
- Ratings
- Attendance
- Location Sharing

---

# Users

Represents every registered user of the application.

| Field | Type | Required | Notes |
|--------|------|----------|-------|
| Id | UUID | Yes | Primary Key |
| Username | String | Yes | Unique |
| Email | String | Yes | Unique |
| PasswordHash | String | Yes | Never store plain passwords |
| FirstName | String | Yes | |
| LastName | String | Yes | |
| ProfilePictureUrl | String | No | |
| BirthDate | Date | No | |
| PreferredPosition | Enum | No | Goalkeeper, Defender, Midfielder, Forward |
| PreferredFoot | Enum | No | Left, Right, Both |
| SkillLevel | Integer | No | Range: 1–10 |
| Reputation | Decimal | Yes | Starts at 100 |
| CreatedAt | DateTime | Yes | |
| LastLogin | DateTime | No | |
| IsActive | Boolean | Yes | |

---

# Friends

Represents friendship relationships between users.

| Field | Type | Required | Notes |
|--------|------|----------|-------|
| Id | UUID | Yes | Primary Key |
| UserId | UUID | Yes | References Users |
| FriendId | UUID | Yes | References Users |
| Status | Enum | Yes | Pending, Accepted, Blocked |
| CreatedAt | DateTime | Yes | |

---

# Groups

Represents a custom group of players created by a user.

Examples:

- Friday Friends
- Work Friends
- University Friends

| Field | Type | Required | Notes |
|--------|------|----------|-------|
| Id | UUID | Yes | Primary Key |
| OwnerId | UUID | Yes | References Users |
| Name | String | Yes | |
| Description | String | No | |
| CreatedAt | DateTime | Yes | |

---

# Group Members

Represents membership of users inside groups.

| Field | Type | Required | Notes |
|--------|------|----------|-------|
| GroupId | UUID | Yes | References Groups |
| UserId | UUID | Yes | References Users |

---

# Football Fields

See:

docs/FootballField.md

Represents football fields where matches are played.

| Field | Type | Required | Notes |
|--------|------|----------|-------|
| Id | UUID | Yes | Primary Key |
| Name | String | Yes | |
| Address | String | Yes | |
| Latitude | Decimal | Yes | |
| Longitude | Decimal | Yes | |
| Indoor | Boolean | Yes | |
| Surface | Enum | Yes | Turf, Grass, Concrete... |
| Capacity | Integer | Yes | 10, 12, 14, 16... |
| DurationMinutes | Integer | Yes | Usually 90 |
| Price | Decimal | Yes | |
| ReservationFee | Decimal | Yes | |
| PhoneNumber | String | No | |
| Parking | Boolean | No | |
| Shower | Boolean | No | |
| Lights | Boolean | Yes | |
| Notes | String | No | |
| CreatedBy | UUID | Yes | References Users |

---

# Matches

Not yet designed.

---

# Match Participants

Not yet designed.

---

# Payments

Not yet designed.

---

# Notifications

Not yet designed.

---

# Chats

Not yet designed.

---

# Messages

Not yet designed.

---

# Ratings

Not yet designed.

---

# Attendance

Not yet designed.

---

# Location Sharing

Not yet designed.

---

# Remaining Work

The following entities will be fully designed before implementation begins:

- Matches
- Match Participants
- Payments
- Notifications
- Chats
- Messages
- Ratings
- Attendance
- Location Sharing

---

> This document is intentionally incomplete and will evolve before implementation begins.