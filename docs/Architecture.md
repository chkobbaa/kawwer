# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Architecture

## Purpose

This document defines the overall software architecture for Kawwer.

All implementation must follow this architecture unless an approved Architecture Decision Record (ADR) changes it.

---

# Architecture Style

Kawwer uses:

- Clean Architecture
- Domain-Driven Design (Lightweight)
- CQRS (without Event Sourcing)
- Repository Pattern
- Dependency Injection

---

# Technology Stack

## Mobile

- .NET MAUI
- MVVM
- CommunityToolkit.Mvvm

---

## Backend

- ASP.NET Core Web API
- .NET 10

---

## Database

- PostgreSQL
- Entity Framework Core

---

## Authentication

- JWT Access Tokens
- Refresh Tokens

---

## Push Notifications

- Firebase Cloud Messaging (FCM)

---

## Mapping

- Mapster

---

## Validation

- FluentValidation

---

## Logging

- Microsoft.Extensions.Logging

---

# Solution Structure

```
Kawwer.sln

Kawwer.Mobile
Kawwer.Api
Kawwer.Application
Kawwer.Domain
Kawwer.Infrastructure
Kawwer.Contracts

Kawwer.Tests
```

---

# Layer Responsibilities

## Kawwer.Domain

Contains:

- Entities
- Value Objects
- Domain Services
- Domain Events
- Enums

Never contains:

- EF Core
- HTTP
- Controllers
- Database code

---

## Kawwer.Application

Contains:

- Use Cases
- Commands
- Queries
- Validators
- Interfaces
- DTO Mapping

Never contains:

- SQL
- Controllers
- EF Core DbContext

---

## Kawwer.Infrastructure

Contains:

- EF Core
- PostgreSQL
- Authentication
- Notifications
- File Storage
- Repository Implementations

---

## Kawwer.Api

Contains:

- Controllers
- Authentication
- Dependency Injection
- Middleware
- Swagger
- API Versioning

Contains no business logic.

---

## Kawwer.Mobile

Contains:

- Views
- ViewModels
- Services
- Local Preferences
- Navigation

Contains no business logic.

---

## Kawwer.Contracts

Contains:

- Request DTOs
- Response DTOs
- Shared Enums
- API Models

No business logic.

---

# Dependency Rules

Allowed:

```
Mobile
↓

API

↓

Application

↓

Domain

Infrastructure
↑
```

Application depends on Domain.

Infrastructure depends on Application and Domain.

API depends on:

- Application
- Infrastructure
- Contracts

Mobile communicates only through HTTP APIs.

---

# Forbidden Dependencies

Domain must never reference:

- Infrastructure
- API
- Mobile

Application must never reference:

- API
- Mobile

Mobile must never reference:

- Domain
- Infrastructure

---

# Dependency Injection

Dependency Injection is configured only inside:

Kawwer.Api

Infrastructure registers:

- Repositories
- Database
- Authentication
- Notifications

Application registers:

- Validators
- Services

---

# CQRS

Every feature uses:

Command

or

Query

Never both.

Examples:

CreateMatchCommand

JoinMatchCommand

CancelMatchCommand

GetUpcomingMatchesQuery

GetProfileQuery

---

# Repository Pattern

Repositories exist only for Aggregate Roots.

Example:

IUserRepository

IMatchRepository

IGroupRepository

No repository for simple lookup tables.

---

# Entity Framework

Migrations exist only inside:

Kawwer.Infrastructure

The API never references EF directly.

---

# Configuration

Configuration comes from:

- appsettings.json
- appsettings.Development.json
- Environment Variables

Secrets are never committed to Git.

---

# Error Handling

All API errors return ProblemDetails.

Validation errors return HTTP 400.

Unauthorized requests return HTTP 401.

Forbidden requests return HTTP 403.

Unhandled exceptions return HTTP 500.

---

# Logging

Every request logs:

- Timestamp
- Endpoint
- User ID
- Duration
- Status Code

Sensitive information is never logged.

---

# Security

Passwords are hashed.

JWT expiration:

15 minutes.

Refresh Tokens:

30 days.

HTTPS required.

---

# Caching

Version 1

No distributed cache.

Future:

Redis.

---

# Real-Time Features

Version 1 uses:

SignalR

For:

- Live Match
- Live Payments
- Live Waiting List
- Match Chat

---

# File Storage

Version 1

Local file storage.

Future:

Cloud storage.

---

# Testing Strategy

Unit Tests

- Domain
- Application

Integration Tests

- API

Architecture Tests

- Dependency rules

---

# Coding Standards

- Nullable Reference Types enabled.
- Async methods end with Async.
- One public class per file.
- Constructor Injection only.
- No static service classes.
- No business logic inside controllers.

---

# Acceptance Criteria

- Every layer has a single responsibility.
- Dependency rules are respected.
- Mobile communicates only through API.
- Infrastructure contains all external integrations.
- Domain contains only business rules.