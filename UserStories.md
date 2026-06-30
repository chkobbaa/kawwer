# Kawwer — User Stories

**Status:** Approved · **Version:** 1.0 · **Last Updated:** 2026-06-30 · **Owner:** Kawwer Team

---

User stories for Version 1, grouped by feature. Each story follows *As a … I want … so that …*
and carries acceptance criteria. Detailed behavior lives in the matching `docs/` document.

## Authentication

- **US-1** As a new user, I want to register with my email and a password so that I can use the
  app. *Accept:* duplicate username/email is rejected; I receive access and refresh tokens.
- **US-2** As a returning user, I want to log in with my username or email so that I can reach my
  matches. *Accept:* wrong credentials fail; repeated failures lock the account temporarily.
- **US-3** As a user, I want to edit my profile and set its visibility so that I control what
  others see.

## Friends & Groups

- **US-4** As an organizer, I want to send and accept friend requests so that I can quickly invite
  the same people. *Accept:* a request can be accepted or rejected; I can block a user.
- **US-5** As an organizer, I want to create groups like "Friday Friends" so that I can invite a
  whole group at once. *Accept:* groups are private to me; I can add/remove members.

## Football Fields

- **US-6** As an organizer, I want to save a field with its price, duration and amenities so that
  I don't re-enter it each time. *Accept:* later price edits don't change past matches.
- **US-7** As an organizer, I want to search saved fields so that I can pick one fast.

## Creating & Managing Matches

- **US-8** As an organizer, I want to create a match from a field so that its details fill in
  automatically. *Accept:* price, fee and duration are snapshotted at creation.
- **US-9** As an organizer, I want to edit, change capacity, cancel or finish a match so that I
  can manage changes. *Accept:* capacity can't drop below accepted players; closed matches can't
  be edited.

## Invitations, Responses & Waiting List

- **US-10** As an organizer, I want to invite players and groups so that spots fill quickly.
- **US-11** As a player, I want to accept or decline an invitation in one tap so that the
  organizer knows my answer. *Accept:* accepting a full match puts me on the waiting list.
- **US-12** As a waiting player, I want to be promoted automatically when someone leaves so that I
  get a fair spot. *Accept:* promotion follows first-accepted, first-promoted order.

## Live Match

- **US-13** As an organizer, I want a match-day screen with attendance and payment progress so
  that I stop calling everyone. *Accept:* attendance is editable until the match finishes.
- **US-14** As a player, I want to optionally share my live location and mark myself arrived so
  that the organizer can see who's on the way. *Accept:* sharing is optional and stops when the
  match ends.

## Payments

- **US-15** As an organizer, I want the per-player share computed automatically so that collection
  is fair and fast. *Accept:* share = remaining ÷ accepted players, rounded up.
- **US-16** As an organizer, I want to record full or partial cash payments and undo mistakes so
  that the ledger is accurate. *Accept:* collection finishes only when the remaining amount is
  zero, after which it is read-only.
- **US-17** As an organizer, I want to split the remaining balance among unpaid players so that I
  cover a shortfall fairly.

## Match Chat

- **US-18** As a participant, I want a chat for the match so that we can coordinate.
  *Accept:* waiting-list players read only; I can edit/delete my message within five minutes.
- **US-19** As an organizer, I want to pin one message so that key instructions stay on top.

## Public Matches

- **US-20** As a player, I want to discover nearby public matches with open spots so that I can
  play more. *Accept:* filters by distance and date; private matches stay hidden.
- **US-21** As an organizer, I want to approve or reject join requests so that I control who
  plays, with an optional auto-accept setting.

## Notifications

- **US-22** As a user, I want push and in-app notifications for invitations, changes, payments and
  promotions so that I never miss an update. *Accept:* every push is also stored in-app; critical
  ones can't be disabled.

## Ratings, Reputation & Statistics

- **US-23** As a player, I want to rate the organizer and players after a match so that reliable
  people are recognized. *Accept:* ratings are 1–5 stars, anonymous, one per rater/ratee/type.
- **US-24** As a user, I want a reliability badge and statistics derived from real participation so
  that my track record is trustworthy. *Accept:* statistics can't be edited manually.

## Calendar

- **US-25** As an accepted player, I want to add the match to my device calendar with reminders so
  that I don't forget. *Accept:* the entry updates or is removed when the match changes or is
  cancelled.
