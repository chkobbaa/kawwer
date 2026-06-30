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