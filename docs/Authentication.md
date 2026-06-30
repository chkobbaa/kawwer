# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Authentication

## Purpose

Authentication allows users to securely create an account, sign in, and access the Kawwer platform.

The authentication system must be secure, simple, and scalable.

---

# Authentication Method

Version 1 supports:

- Email
- Username
- Password

Future versions may support:

- Google
- Apple
- Facebook

---

# Registration

A new user must provide:

| Field | Required |
|--------|----------|
| Username | Yes |
| Email | Yes |
| Password | Yes |
| First Name | Yes |
| Last Name | Yes |
| Birth Date | No |
| Preferred Position | No |
| Preferred Foot | No |

Passwords are never stored.

Only password hashes are stored.

---

# Login

Users may login using:

- Username
or
- Email

and

Password

---

# Password Rules

Minimum length:

8 characters

Must contain:

- Uppercase letter
- Lowercase letter
- Number

Special characters are recommended but not required.

---

# Password Reset

User enters email.

↓

Receives reset email.

↓

Clicks secure link.

↓

Creates new password.

↓

Previous sessions are invalidated.

---

# Session Management

After login:

The backend returns:

- Access Token
- Refresh Token

The mobile application stores them securely.

---

# Logout

Logging out removes locally stored tokens.

---

# Failed Login Attempts

Five consecutive failed login attempts temporarily lock the account for:

15 minutes

---

# Account Status

Possible values:

- Active
- Suspended
- Deleted

Deleted accounts are soft deleted.

---

# Security Requirements

Passwords:

- Never stored
- Never logged
- Never returned

Connections:

- HTTPS only

Tokens:

- Signed
- Expiring

Refresh tokens:

- Revocable

---

# Future Features

- Two-factor authentication
- Biometric login
- Passkeys
- Device management
- Login history