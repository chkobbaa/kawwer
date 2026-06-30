# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Groups System

## Purpose

Groups allow organizers to organize their friends into reusable collections.

Instead of selecting players individually every time a match is created, organizers can invite an entire group with a single action.

Groups are private and visible only to their owner.

---

# Group

A Group contains:

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

Every group has exactly one owner.

Only the owner may:

- Rename the group
- Delete the group
- Add members
- Remove members

---

# Members

A group may contain:

Minimum:

0 players

Maximum:

No limit

A player may belong to multiple groups.

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

Removing a player from a group:

- Does not remove the friendship.
- Does not affect previous matches.
- Does not affect future invitations already sent.

---

# Deleting Groups

Deleting a group:

- Removes only the group.
- Does not remove friendships.
- Does not remove matches.
- Does not remove invitations.

Deletion requires confirmation.

---

# Invitations

When creating a match, the organizer may choose:

- Individual friends
- One group
- Multiple groups
- Everyone

Duplicate invitations must never be created.

If the same player belongs to multiple selected groups, only one invitation is sent.

---

# Business Rules

- Groups are private.
- Only the owner can modify a group.
- Friends may belong to multiple groups.
- Duplicate members are not allowed.
- Empty groups are allowed.

---

# Validation Rules

Group Name

Minimum:

2 characters

Maximum:

50 characters

Description

Maximum:

250 characters

---

# Future Extensions

- Shared groups
- Public groups
- Smart groups
- Import from previous matches
- Favorite groups
- Automatic player suggestions

---

# Acceptance Criteria

- A user can create a group.
- A user can rename a group.
- A user can delete a group.
- A user can add friends.
- A user can remove friends.
- Duplicate members are prevented.
- Selecting multiple groups never sends duplicate invitations.