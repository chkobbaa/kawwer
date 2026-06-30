# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Match Chat

## Purpose

Match Chat provides a temporary conversation space for participants of a specific match.

Its purpose is to coordinate the match before and during gameplay.

The chat is not intended to replace general messaging applications.

---

# Availability

The chat becomes available immediately after a match is published.

The chat becomes read-only when:

- The match is cancelled.
- The match is archived.

---

# Participants

Only accepted participants may send messages.

The organizer always has access.

Waiting list players may read messages but cannot send them.

Declined players have no access.

---

# Message Types

Version 1 supports:

- Text messages
- Emoji reactions

Future versions may support:

- Images
- Voice messages
- File attachments
- Polls

---

# Organizer Messages

The organizer may pin one message.

Pinned messages always appear at the top of the chat.

Examples:

- Bring white shirts.
- Arrive 15 minutes early.
- Parking entrance has changed.

---

# System Messages

The application automatically posts events such as:

- Ahmed joined the match.
- Mohamed left the match.
- Ali was promoted from the waiting list.
- Payment collection has started.
- Live Match has started.
- Match has been cancelled.

System messages cannot be deleted.

---

# User Messages

Participants may:

- Send messages
- Edit messages within five minutes
- Delete their own messages within five minutes

After five minutes:

Messages become permanent.

---

# Mentions

Users may mention:

@Username

Mentioned users receive a notification.

---

# Read Receipts

The organizer can view:

- Number of participants who have read each message.

Individual read receipts remain private.

---

# Search

Users may search messages by:

- Keyword
- Sender

---

# Business Rules

- One chat exists per match.
- Chats are archived with the match.
- Waiting list players cannot send messages.
- Deleted messages are soft deleted.
- System messages cannot be modified.

---

# Future Extensions

- Polls
- Photos
- Voice notes
- Team selection voting
- Match highlights

---

# Acceptance Criteria

- Accepted participants can send messages.
- Waiting list players have read-only access.
- Organizer can pin messages.
- System messages appear automatically.
- Chats become read-only after archival.