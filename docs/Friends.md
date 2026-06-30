# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Friends System

## Purpose

The Friends System allows users to connect with each other.

Friends make it easier to invite players to matches without searching for them every time.

---

# Friendship Lifecycle

No Relationship

↓

Friend Request Sent

↓

Accepted

or

Rejected

↓

Blocked (optional)

---

# Friend Request

A user may send a friend request to another registered user.

The receiver may:

- Accept
- Reject

A request remains pending until one of these actions occurs.

---

# Removing Friends

Either user may remove the friendship.

Removing a friendship:

- Removes both users from each other's friends list.
- Does not delete previous matches.
- Does not delete previous chat messages.

---

# Blocking

A blocked user:

- Cannot send friend requests.
- Cannot invite the blocker to private matches.
- Cannot send direct messages (future feature).
- Cannot view private profile information.

Blocking automatically removes an existing friendship.

---

# Searching

Users may search by:

- Username
- First Name
- Last Name

Search results never reveal email addresses.

---

# Privacy

Every user has three profile visibility options.

Public

Anyone can view.

Friends Only

Only friends may view.

Private

Only the owner may view.

---

# Invitations

Organizers may invite:

- Individual friends
- Entire groups
- Everyone in their friends list

---

# Public Players

A player who enables Public Profile may receive invitations to public matches from nearby organizers.

This feature may be disabled in account settings.

---

# Business Rules

- Users cannot friend themselves.
- Duplicate friend requests are not allowed.
- Duplicate friendships are not allowed.
- Blocking always removes friendship.
- Removing friendship does not affect match history.

---

# Validation Rules

Username search:

Minimum:

2 characters

Maximum:

50 characters

---

# Future Extensions

- Mutual friends
- Friend recommendations
- Recently played together
- Favorite teammates
- Direct messaging

---

# Acceptance Criteria

- A user can send a friend request.
- A user can accept a friend request.
- A user can reject a friend request.
- A user can remove a friend.
- A blocked user cannot send invitations.
- Duplicate friendships are prevented.