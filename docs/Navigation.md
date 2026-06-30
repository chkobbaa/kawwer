# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Navigation

## Purpose

This document defines how users move through the Kawwer mobile application.

The navigation must be simple, predictable, and optimized for quick access to football matches.

---

# Navigation Style

Version 1 uses:

- Bottom Navigation Bar
- Stack Navigation
- Modal Pages

---

# Bottom Navigation

Five primary tabs.

## Home

Organizer dashboard.

Contains:

- Upcoming matches
- Next match countdown
- Quick actions
- Notifications summary

---

## Discover

Contains:

- Public matches
- Search
- Filters
- Nearby football fields

---

## Calendar

Contains:

- Monthly calendar
- Upcoming matches
- Match history shortcuts

---

## Friends

Contains:

- Friends
- Groups
- Invitations
- Favorite groups

---

## Profile

Contains:

- User profile
- Statistics
- Reputation
- Settings
- Saved football fields

---

# Home Flow

Home

↓

Upcoming Match

↓

Live Match

↓

Payments

↓

Match Chat

---

# Discover Flow

Discover

↓

Filters

↓

Match Details

↓

Join Request

↓

Waiting List (if applicable)

---

# Calendar Flow

Calendar

↓

Selected Day

↓

Match Details

↓

Live Match

---

# Friends Flow

Friends

↓

Friend Profile

↓

Invite to Match

or

Create Group

---

# Profile Flow

Profile

↓

Statistics

↓

Reputation

↓

Settings

↓

Saved Football Fields

---

# Match Details

Displays:

- Organizer
- Date
- Time
- Football Field
- Accepted Players
- Waiting List
- Payment Status
- Chat
- Live Match
- Calendar Entry

---

# Quick Actions

From Home:

- Create Match
- View Live Match
- Discover Matches
- Invite Friends
- Open Calendar

---

# Floating Action Button

Visible only on:

Home

Action:

Create Match

---

# Deep Links

Supported:

- Match Invitation
- Public Match
- Friend Request

Future:

Shared calendar links.

---

# Back Navigation

Android

Uses system Back button.

iOS

Uses navigation bar Back button.

Unsaved changes require confirmation before leaving.

---

# Authentication Flow

Splash Screen

↓

Login

↓

Register

↓

Home

Returning users:

Splash

↓

Home

---

# Organizer Shortcuts

When the logged-in user organizes an upcoming match:

Home displays:

- Start Live Match
- Open Payments
- Open Match Chat

---

# Notification Navigation

Selecting a notification opens the related screen.

Examples:

Invitation

↓

Match Details

Payment

↓

Payments

Friend Request

↓

Friends

---

# Acceptance Criteria

- Bottom navigation provides access to all major features.
- Navigation follows platform conventions.
- Deep links open the correct screens.
- Quick actions are accessible from Home.
- Organizer actions require minimal navigation.