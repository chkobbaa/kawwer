# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Calendar Integration

## Purpose

The Calendar Integration keeps players informed about upcoming matches by synchronizing accepted matches with their device calendar.

The goal is to reduce missed matches and scheduling conflicts.

---

# Supported Calendars

Version 1 supports:

- Android Calendar
- iOS Calendar

No cloud calendar integration is required.

Future versions may support:

- Google Calendar
- Apple Calendar (iCloud)
- Outlook

---

# Calendar Entry

When a player accepts a match, the application offers:

Add to Calendar

If enabled in Settings, this action may occur automatically after acceptance.

Each calendar event includes:

- Match title
- Football field name
- Address
- Date
- Start time
- End time
- Organizer name
- Notes
- Navigation link

---

# Updating Events

If the organizer changes:

- Date
- Start time
- Football field
- Match duration

The calendar entry is automatically updated.

---

# Cancelling Events

If a match is cancelled:

- The calendar entry is removed automatically.

If automatic removal fails, the player receives a notification asking them to remove the event manually.

---

# Reminder Times

Default reminders:

- 24 hours before kickoff
- 3 hours before kickoff
- 30 minutes before kickoff

Users may customize reminder times in Settings.

---

# Conflict Detection

When creating or joining a match, the application checks for overlaps with:

- Other Kawwer matches

Future versions may also check the device calendar.

If a conflict exists, the user receives a warning.

The user may still continue.

---

# Navigation

Every calendar entry includes:

Open Navigation

Selecting this option opens the user's preferred navigation application.

---

# Business Rules

- Only accepted participants may create calendar entries.
- Waiting list players do not receive calendar events.
- Calendar updates follow match changes automatically.
- Cancelled matches remove calendar events.

---

# Acceptance Criteria

- Accepted players can add matches to their calendar.
- Calendar entries update after match changes.
- Cancelled matches remove calendar events.
- Reminder notifications are scheduled correctly.
- Conflict warnings are displayed when appropriate.