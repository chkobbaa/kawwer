# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Notifications System

## Purpose

The Notifications System keeps players informed about important match events without requiring organizers to contact everyone manually.

Notifications must be timely, relevant, and never spam users.

---

# Delivery Methods

Version 1 supports:

- Push Notifications
- In-App Notifications

Future versions may support:

- SMS
- Email
- WhatsApp

---

# Notification Categories

## Match

- Match created
- Match updated
- Match cancelled
- Match full
- Match started
- Match finished

---

## Invitations

- New invitation
- Invitation reminder
- Waiting list promotion
- Invitation accepted
- Invitation declined

---

## Payments

- Collection started
- Payment completed
- Remaining balance updated

---

## Live Match

- Live Match started
- Location request
- Match begins in 30 minutes
- Match begins in 10 minutes

---

## Friends

- Friend request received
- Friend request accepted

---

## Groups

- Added to group (future shared groups)

---

# Organizer Notifications

Organizers receive notifications when:

- A player accepts.
- A player declines.
- A player joins the waiting list.
- A player leaves the match.
- Payment collection completes.
- A participant becomes a no-show.

---

# Player Notifications

Players receive notifications when:

- Invited to a match.
- Promoted from the waiting list.
- Match details change.
- Match is cancelled.
- Live location is requested.
- Payment collection begins.

---

# Notification Preferences

Users may enable or disable:

- Match reminders
- Friend requests
- Public match invitations
- Live Match notifications

Critical notifications cannot be disabled:

- Match cancellation
- Waiting list promotion

---

# Notification Center

Every notification is stored inside the application.

Each notification contains:

| Field | Description |
|--------|-------------|
| Title | Short summary |
| Message | Notification body |
| Category | Match, Payment, Friend, etc. |
| Created At | Timestamp |
| Read | Yes / No |

Users may:

- Mark as read.
- Mark all as read.
- Delete notifications.

---

# Business Rules

- Notifications are never duplicated.
- Read status is synchronized across devices.
- Push notifications always create an in-app notification.
- Notification history is retained for 90 days.

---

# Acceptance Criteria

- Invitations trigger notifications.
- Match updates trigger notifications.
- Payment collection triggers notifications.
- Waiting list promotions trigger notifications.
- Notification preferences are respected.
- Read status synchronizes correctly.