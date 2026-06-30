# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# API Standards

## Purpose

This document defines the standards followed by every REST endpoint in Kawwer.

Feature documents define the behavior.

This document defines how APIs behave.

---

# API Style

Kawwer uses:

REST API

JSON

HTTPS

UTF-8

---

# Base URL

Development

```

https://localhost:5001/api/v1

```

Production

```

https://api.kawwer.com/api/v1

```

---

# API Versioning

Every endpoint begins with:

```

/api/v1/

```

Future breaking changes use:

```

/api/v2/

```

---

# Authentication

Protected endpoints require:

Bearer JWT Token

Example

```

Authorization: Bearer eyJhb...

```

Public endpoints:

- Login
- Register
- Refresh Token

---

# HTTP Methods

GET

Retrieve data

POST

Create resources

PUT

Replace resources

PATCH

Partial updates

DELETE

Delete resources

---

# Standard Response

Successful response

```json
{
  "success": true,
  "data": {},
  "message": null
}
```

---

# Standard Error

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    "Name is required."
  ]
}
```

---

# Status Codes

200 OK

201 Created

204 No Content

400 Bad Request

401 Unauthorized

403 Forbidden

404 Not Found

409 Conflict

422 Unprocessable Entity

500 Internal Server Error

---

# Pagination

Every list endpoint supports:

page

pageSize

Maximum page size:

100

Response

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 420,
  "totalPages": 21
}
```

---

# Filtering

Common filters:

search

dateFrom

dateTo

status

sortBy

sortDirection

---

# Sorting

Ascending

```

asc

```

Descending

```

desc

```

---

# Date Format

ISO-8601 UTC

Example

```

2026-06-30T18:45:00Z

```

---

# Validation

Validation uses FluentValidation.

Invalid requests return HTTP 400.

---

# Problem Details

Unexpected errors return:

ProblemDetails

RFC 9457 compatible.

---

# Idempotency

GET

Safe

PUT

Idempotent

DELETE

Idempotent

POST

Not idempotent

---

# Security

HTTPS only.

JWT required.

Refresh Tokens supported.

Passwords never returned.

Sensitive data never logged.

---

# File Uploads

Multipart Form Data.

Maximum upload size:

10 MB

Supported:

JPEG

PNG

Future:

HEIC

---

# Rate Limiting

Version 1

No rate limiting.

Future:

100 requests/minute/user.

---

# OpenAPI

Swagger enabled.

OpenAPI 3.1

Every endpoint documented.

---

# Naming

Resources use plural nouns.

Examples

/users

/matches

/groups

/football-fields

---

# Acceptance Criteria

- Every endpoint follows REST conventions.
- Every protected endpoint requires JWT.
- Responses follow the standard response format.
- Errors follow the standard error format.
- Pagination behaves consistently.