# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Screens

## Purpose

This document defines every screen in the Kawwer mobile application.

Each screen has a single responsibility and a clearly defined navigation flow.

---

# Authentication

## Splash Screen

Purpose

- Load application.
- Check authentication.
- Restore session.

Actions

- Navigate to Login.
- Navigate to Home.

---

## Login

Purpose

Authenticate existing users.

Fields

- Email
- Password

Actions

- Login
- Forgot Password
- Register

---

## Register

Purpose

Create a new account.

Fields

- First Name
- Last Name
- Username
- Email
- Phone Number
- Password
- Confirm Password

Actions

- Register
- Back to Login

---

# Home

## Dashboard

Purpose

Main landing page.

Displays

- Next Match
- Upcoming Matches
- Notifications Summary
- Quick Actions
- Match Confidence Alerts

Actions

- Create Match
- Open Live Match
- Discover Matches

---

# Matches

## Create Match

Displays

- Match Name
- Football Field
- Date
- Time
- Duration
- Player Count
- Visibility
- Invitation Groups

Actions

- Create
- Save Draft
- Cancel

---

## Match Details

Displays

- Match Information
- Players
- Waiting List
- Payment Summary
- Organizer
- Chat
- Live Match

Actions

- Accept
- Decline
- Leave Match
- Open Chat
- Open Payments

---

## Waiting List

Displays

- Position
- Estimated Promotion
- Match Information

Actions

- Leave Waiting List

---

## Live Match

Displays

- Live Map
- Attendance
- Payments
- Arrival Status
- Match Progress

Actions

- Share Location
- Mark Arrived
- Open Navigation
- Open Chat

Organizer Actions

- Start Collection
- Request Locations
- Send Reminder

---

## Payments

Displays

- Amount Required
- Amount Collected
- Remaining Amount
- Player Statuses

Actions

- Record Payment
- Undo Payment
- Finish Collection

---

## Match Chat

Displays

- Messages
- Pinned Message
- System Events

Actions

- Send Message
- React
- Mention Player

Organizer Actions

- Pin Message

---

# Discover

## Discover Matches

Displays

- Public Matches
- Filters
- Search

Actions

- View Match
- Join Match

---

## Public Match Details

Displays

- Match Information
- Organizer
- Reliability
- Participants
- Available Spots

Actions

- Request to Join

---

# Friends

## Friends

Displays

- Friends List
- Friend Requests

Actions

- Add Friend
- Remove Friend
- Invite to Match

---

## Groups

Displays

- Favorite Groups
- Group Members

Actions

- Create Group
- Edit Group
- Delete Group

---

# Calendar

## Calendar

Displays

- Monthly Calendar
- Upcoming Matches
- Daily Match List

Actions

- Open Match

---

# Notifications

## Notification Center

Displays

- All Notifications
- Read Status

Actions

- Mark Read
- Mark All Read
- Delete

---

# Profile

## Profile

Displays

- User Information
- Reputation Badge
- Statistics
- Reliability

Actions

- Edit Profile
- Settings

---

## Statistics

Displays

- Matches Played
- Attendance
- Ratings
- Payment Reliability
- Monthly Trends

---

## Saved Football Fields

Displays

- Favorite Fields

Actions

- Add
- Edit
- Delete

---

## Football Field Discovery

Displays

- Nearby Fields
- Search Results

Actions

- Save Field
- View Details

---

## Settings

Displays

- Notification Preferences
- Privacy
- Calendar
- Account

Actions

- Save Changes
- Logout

---

# Error Screens

- No Internet
- Server Error
- Unauthorized
- Not Found

---

# Modal Dialogs

- Leave Match
- Cancel Match
- Delete Group
- Delete Friend
- Start Payment Collection
- Request Live Location

---

# Acceptance Criteria

- Every feature has a dedicated screen.
- Every screen has a single responsibility.
- Navigation between screens is defined.
- Organizer-specific actions are separated from participant actions.