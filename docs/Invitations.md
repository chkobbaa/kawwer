# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Invitations System

## Purpose

The Invitations System manages the complete lifecycle of player invitations for football matches.

It tracks every invited player from the moment a match is published until the match is finished.

---

# Invitation Sources

Players may be invited through:

- Individual selection
- One group
- Multiple groups
- Entire friends list
- Public match discovery

Duplicate invitations are never created.

---

# Invitation Lifecycle

Invited

↓

Notification Sent

↓

Seen

↓

Thinking

↓

Accepted

or

Declined

↓

Waiting List (if match is already full)

↓

Promoted

↓

Match Day

---

# Player Responses

Each invited player may choose one of the following:

## Accept

Immediately joins the match if places remain.

If the match is already full, the player joins the waiting list.

---

## Thinking

The player has seen the invitation but has not yet decided.

The organizer can see this status.

---

## Decline

The player declines the invitation.

The organizer is notified.

The player may later change the response before the response deadline.

---

# Invitation Deadline

Players may freely change their response until:

48 hours before kickoff.

Within the final 48 hours:

- Accepted players receive a warning before leaving.
- If a waiting list exists, the first waiting player is promoted automatically.
- If no waiting list exists, the organizer is warned that the match may become unbalanced.

Leaving is still allowed.

---

# Automatic Promotion

When an accepted player leaves:

If waiting list is not empty:

↓

First waiting player is promoted.

↓

Notification sent.

↓

Organizer notified.

---

# Organizer View

For every invited player the organizer can see:

- Invitation status
- Response time
- Payment status
- Attendance status
- Live location status (when enabled)

---

# Player View

Each invited player can see:

- Match details
- Football field
- Date
- Time
- Accepted player count
- Waiting list position (if applicable)

Players cannot see private organizer information.

---

# Reminder Notifications

The organizer may send manual reminders.

Automatic reminders are sent:

- 24 hours before kickoff
- 3 hours before kickoff
- 30 minutes before kickoff

Only players who have not declined receive reminders.

---

# Business Rules

- Duplicate invitations are forbidden.
- Invitation status is stored in MatchParticipant.
- Invitations may only be sent by the organizer.
- Public matches automatically accept public join requests until full.
- Every invitation creates exactly one MatchParticipant record.

---

# Validation Rules

A player cannot receive:

- More than one invitation for the same match.

A blocked user cannot be invited to a private match.

---

# Acceptance Criteria

- Invitations are sent after publishing.
- Duplicate invitations never occur.
- Accept joins the match.
- Accept joins waiting list when full.
- Promotion occurs automatically.
- Organizer receives updates.
- Players receive reminder notifications.