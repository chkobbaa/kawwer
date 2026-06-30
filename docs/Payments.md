# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Payments System

## Purpose

The Payments System helps organizers collect cash before a match begins.

It tracks every participant's contribution and ensures the total field price has been collected before play starts.

Version 1 supports cash payments only.

---

# Payment Flow

Organizer arrives at the football field

↓

Organizer taps

**Start Collecting Money**

↓

Payment Dashboard opens

↓

Organizer records payments

↓

Remaining amount reaches zero

↓

Collection completed

---

# Automatic Calculations

Remaining Amount

= Total Field Price - Reservation Fee

Share Per Player

= Ceiling(Remaining Amount / Number of Players Excluding Organizer)

Example

Field Price

90 TND

Reservation

5 TND

Remaining

85 TND

Accepted Players (excluding organizer)

13

85 / 13 = 6.54

Each player pays:

7 TND

Organizer keeps any remaining amount caused by rounding.

---

# Payment Dashboard

The organizer sees:

| Information | Description |
|------------|-------------|
| Total Field Price | Full field cost |
| Reservation Paid | Amount already paid |
| Remaining Amount | Still to collect |
| Money Collected | Current collected amount |
| Money Missing | Amount still missing |
| Paid Players | Number of players fully paid |
| Partial Payments | Number of partially paid players |
| Unpaid Players | Number of players not yet paid |

---

# Player Status

Each participant has one payment status.

Possible values:

- Not Paid
- Partially Paid
- Paid

---

# Organizer Actions

For every player:

- Mark as Paid
- Enter custom amount
- Undo payment
- View payment history

Global actions:

- Split remaining balance
- Assign remaining balance
- Finish collection

---

# Partial Payments

Players may pay any amount.

Example:

Required:

7 TND

Paid:

5 TND

Remaining:

2 TND

The organizer decides how to handle the difference.

---

# Remaining Balance Options

When money is still missing, the organizer may:

## Option 1

Split the remaining balance equally among unpaid players.

---

## Option 2

Assign the remaining balance to one or more selected players.

---

## Option 3

Leave the balance assigned to the original player.

---

# Finish Collection

Collection may only finish when:

Remaining Amount = 0

Once completed:

- Payments become read-only.
- Match payment status becomes Completed.

---

# Payment History

Each payment stores:

| Field | Description |
|--------|-------------|
| Player | User |
| Amount | Cash amount |
| Date | Timestamp |
| Recorded By | Organizer |

---

# Business Rules

- Cash only.
- Organizer records all payments.
- Players cannot modify payment records.
- Finished collections cannot be edited.
- Negative payments are forbidden.
- Overpayments are allowed.

---

# Edge Cases

## Player Leaves Before Paying

Organizer may:

- Redistribute unpaid amount.
- Assign unpaid amount.
- Cancel collection and continue later.

---

## Organizer Entered Wrong Amount

Payment may be undone until collection is finished.

---

## Match Cancelled

Existing payment history remains available.

No additional payments may be recorded.

---

# Acceptance Criteria

- Share is calculated automatically.
- Cash payments can be recorded.
- Partial payments are supported.
- Remaining balance updates instantly.
- Finish Collection requires zero remaining balance.
- Organizer can undo payments before completion.