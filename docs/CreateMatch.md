# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Create Match

## Purpose

The Create Match workflow allows an organizer to schedule a football match and invite players.

The workflow must require the minimum number of steps while providing enough flexibility for different match types.

---

# Organizer Workflow

The organizer taps:

Create Match

↓

Select Football Field

↓

Select Date

↓

Select Start Time

↓

Review automatically loaded field information

↓

Enter optional match details

↓

Select visibility

↓

Choose players or groups

↓

Review match summary

↓

Publish Match

---

# Required Information

| Field | Required | Description |
|--------|----------|-------------|
| Football Field | Yes | Existing football field |
| Match Date | Yes | Date of the match |
| Start Time | Yes | Match start time |
| Visibility | Yes | Private or Public |
| Invited Players | Yes | At least one player |

---

# Automatically Loaded

After selecting a football field, the application automatically loads:

- Capacity
- Match duration
- Total price
- Reservation fee
- Remaining amount
- Default GPS location

The organizer may override the price only if the field owner has enabled manual pricing.

---

# Optional Information

The organizer may specify:

- Match title
- Description
- Dress color (future)
- Special instructions

---

# Visibility

Private

Only invited players can see the match.

Public

The match appears in the Public Matches page until all places are filled.

---

# Validation Rules

A match cannot be published if:

- No football field is selected.
- No date is selected.
- No start time is selected.
- No invited players exist.

---

# Publishing

Publishing a match immediately:

- Creates the Match.
- Creates MatchParticipant records.
- Sends push notifications.
- Adds the match to invited players' calendars.

---

# Editing

The organizer may edit:

- Date
- Time
- Description
- Visibility

The organizer may not reduce the maximum player count below the number of accepted players.

---

# Cancelling

Only the organizer may cancel a match.

Cancelling a match:

- Notifies every participant.
- Closes the chat.
- Stops live location sharing.
- Prevents further responses.

---

# Acceptance Criteria

- A match can be created successfully.
- Invalid matches cannot be published.
- Invitations are sent automatically.
- Calendar entries are created.
- Push notifications are sent.
- Duplicate invitations are prevented.