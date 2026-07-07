# Document Information

**Status:** Approved

**Version:** 2.0

**Last Updated:** 2026-07-07

**Owner:** Kawwer Team

---

# Teams System

## Purpose

Teams allow organizers to organize their friends into reusable collections.

Instead of selecting players individually every time a match is created, organizers can invite an entire team with a single action.

Teams are private and visible only to their owner.

> **Note:** Teams were previously called "Groups". The concept is unchanged; the terminology was updated across the app and the public API (`/api/v1/teams`).

---

# Team

A Team contains:

- Name
- Description (optional)
- Members

Examples:

- Friday Night
- Sunday Football
- University Friends
- Work Friends
- Family

---

# Ownership

Every team has exactly one owner.

Only the owner may:

- Rename the team
- Delete the team
- Add members
- Remove members

---

# Members

A team may contain:

Minimum:

0 players

Maximum:

No limit

A player may belong to multiple teams.

Example:

Ali

↓

Friday Night

Work Friends

University

---

# Adding Members

The organizer may add:

- Individual friends

Only users that are already friends may be added.

---

# Removing Members

Removing a player from a team:

- Does not remove the friendship.
- Does not affect previous matches.
- Does not affect future invitations already sent.

---

# Deleting Teams

Deleting a team:

- Removes only the team.
- Does not remove friendships.
- Does not remove matches.
- Does not remove invitations.

Deletion requires confirmation.

---

# Invitations

When creating a match, the organizer may choose:

- Individual friends
- One team
- Multiple teams
- Everyone

Duplicate invitations must never be created.

If the same player belongs to multiple selected teams, only one invitation is sent.

---

# Teams as Match Opponents

A Team can also be designated as the **opponent** of a match. When creating a match the
organizer picks a match format:

- **Pickup match** — everyone who joins is pooled into one game (the default, unchanged behaviour).
- **Play against an external team** — the organizer types the opponent's name; the opponent does not use the app.
- **Play against an app Team** — the organizer selects one of their registered Teams as the opponent.

The opponent is stored on the match itself (`Format`, `OpponentName`, `OpponentTeamId`) and shown
with a default opponent avatar in the match details header.

---

# Business Rules

- Teams are private.
- Only the owner can modify a team.
- Friends may belong to multiple teams.
- Duplicate members are not allowed.
- Empty teams are allowed.

---

# Validation Rules

Team Name

Minimum:

2 characters

Maximum:

50 characters

Description

Maximum:

250 characters

---

# Future Extensions

- Shared teams
- Public teams
- Smart teams
- Import from previous matches
- Favorite teams
- Automatic player suggestions
- Cross-user Team search when selecting an opponent Team

---

# Acceptance Criteria

- A user can create a team.
- A user can rename a team.
- A user can delete a team.
- A user can add friends.
- A user can remove friends.
- Duplicate members are prevented.
- Selecting multiple teams never sends duplicate invitations.
- An organizer can designate an external or in-app Team as the match opponent.
