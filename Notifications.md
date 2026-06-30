# Notifications — Overview

**Status:** Approved · **Owner:** Kawwer Team

Kawwer keeps players informed without the organizer messaging everyone by hand. Version 1 delivers
**push notifications** (Firebase Cloud Messaging) and **in-app notifications** — and every push
also creates a persistent in-app record (decision D-016), so nothing is lost to an accidentally
dismissed banner.

Categories include Match, Invitation, Payment, LiveMatch, Friend, Group and WaitingList. Users can
mark notifications read, mark all read and delete them, and can tune preferences — but critical
notifications (match cancellation, waiting-list promotion) cannot be disabled.

**→ Full categories, organizer/player triggers, preferences, retention and acceptance criteria:
[`docs/Notifications.md`](docs/Notifications.md).** The persisted record is the `notifications`
table in [`docs/Database.md`](docs/Database.md).
