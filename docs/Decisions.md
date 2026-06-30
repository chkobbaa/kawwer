# Decisions

## D-001

**Decision:** Use .NET MAUI for the mobile application.

**Reason:** Single codebase for Android and iOS.

---

## D-002

**Decision:** Use PostgreSQL as the primary database.

**Reason:** Reliable, open-source, excellent support for relational data.

---

## D-003

**Decision:** Store invitations, waiting list, attendance, payment, and response state in a single `MatchParticipant` entity.

**Reason:** Simplifies the data model and reduces duplication.

---

## D-004

**Decision:** Use a single `MatchParticipant` entity instead of separate Invitation, WaitingList, Attendance, and Payment entities.

**Reason:** A participant's lifecycle naturally evolves through invitation, response, payment, attendance, and rating. Keeping this information in one entity reduces duplication and simplifies queries.

---

## D-005

**Decision:** Football fields are independent reusable entities.

**Reason:** A football field can host many matches and should only be entered once. Price changes must not affect previously created matches.

---

## D-006

**Decision:** Version 1 authentication uses email/username and password with JWT access and refresh tokens.

**Reason:** This provides a secure authentication mechanism while keeping the initial implementation simple. Social login can be added later without changing the domain model.

---

## D-007

**Decision:** Friendships are mutual relationships created through a friend request process.

**Reason:** Organizers frequently invite the same players. Maintaining a friend list reduces repetitive searching while preserving user privacy.

---

## D-008

**Decision:** Groups are private and owned by a single organizer.

**Reason:** Groups exist only to simplify player selection during match creation. Shared ownership introduces unnecessary complexity for Version 1.

---

## D-009

**Decision:** Match creation is a guided workflow that automatically loads field information.

**Reason:** Organizers create matches frequently. Automatic loading reduces repetitive data entry and minimizes mistakes.

---

## D-010

**Decision:** The Organizer Dashboard is part of Version 1.

**Reason:** Organizers need a central place to monitor player confirmations, payments, reminders, and match progress. This is a core workflow rather than an optional enhancement.

---

## D-011

**Decision:** Match Templates and Recurring Matches are postponed to Version 2.

**Reason:** They improve convenience but are not required for the initial release. Delaying them keeps Version 1 focused while leaving room for future expansion.

---

## D-012

**Decision:** Invitations are represented through the MatchParticipant entity instead of a dedicated Invitation entity.

**Reason:** A participant naturally progresses through invitation, response, waiting list, attendance, payment, and rating. A single lifecycle entity avoids duplication and simplifies implementation.

---

## D-013

**Decision:** Waiting lists use a strict first-accepted, first-promoted policy.

**Reason:** This approach is transparent, predictable, and easy for users to understand while preventing disputes about promotion order.

---

## D-014

**Decision:** Version 1 supports cash payments only.

**Reason:** Football matches in Tunisia are typically paid in cash immediately before play. Supporting this workflow keeps the application simple and closely aligned with real-world usage.

---

## D-015

**Decision:** Replace a standalone GPS feature with a Live Match control center.

**Reason:** Organizers need more than location tracking. Combining attendance, payments, reminders and optional live location into a single screen creates a much more useful match-day experience.

---

## D-016

**Decision:** Every push notification also creates a persistent in-app notification.

**Reason:** Users may dismiss push notifications accidentally. Keeping a history inside the application ensures important information remains accessible.

---

