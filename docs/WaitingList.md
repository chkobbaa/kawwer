# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Waiting List System

## Purpose

The Waiting List System ensures that football matches remain full by automatically filling vacant spots when accepted players leave.

It provides a fair, predictable, and transparent process for promoting players into the match.

---

# Overview

When all available player spots have been filled, additional players who accept an invitation are automatically placed on the waiting list.

The waiting list is ordered by acceptance time.

The first player on the waiting list is always promoted first when a place becomes available.

---

# Joining the Waiting List

A player joins the waiting list when:

- The match has reached its maximum player capacity.
- The player selects **Accept**.

The application displays:

- Waiting list position.
- Number of players ahead.
- Match information.

---

# Waiting List Order

Waiting list priority is determined by:

1. Acceptance timestamp.

If two timestamps are identical, the player with the lower User ID is promoted first.

---

# Automatic Promotion

When an accepted player leaves the match:

↓

The first waiting player is promoted.

↓

The promoted player receives a push notification.

↓

The organizer receives a push notification.

↓

Remaining waiting positions are recalculated automatically.

---

# Leaving the Waiting List

A player may leave the waiting list at any time before promotion.

Leaving removes the player completely from the waiting list.

The remaining positions are updated immediately.

---

# Organizer View

The organizer can see:

- Current waiting list.
- Waiting order.
- Join time.
- Player reputation.
- Player status.

The organizer cannot manually reorder the waiting list.

---

# Player View

A waiting player can see:

- Current position.
- Total waiting players.
- Match date.
- Match time.
- Football field.
- Current accepted player count.

Players cannot see other waiting players' personal information.

---

# Notifications

The application sends notifications when:

- A player joins the waiting list.
- A player is promoted.
- Waiting position changes.
- The match is cancelled.

---

# Business Rules

- Waiting list order is automatic.
- Manual reordering is not allowed.
- Promotion is immediate.
- Promotion always follows waiting list order.
- Promotion creates no duplicate MatchParticipant records.

---

# Edge Cases

## Organizer Cancels Match

The waiting list is deleted.

All waiting players receive a cancellation notification.

---

## Organizer Increases Capacity

Waiting players are promoted automatically until all new places are filled.

---

## Organizer Decreases Capacity

Capacity cannot be reduced below the number of currently accepted players.

---

## Promoted Player Declines

The next waiting player is promoted immediately.

---

# Acceptance Criteria

- Full matches create waiting lists.
- Waiting order follows acceptance time.
- Promotion is automatic.
- Notifications are sent.
- Waiting positions update automatically.
- Organizer cannot reorder the waiting list.