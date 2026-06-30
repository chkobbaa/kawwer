# FootballField Entity

## Purpose

A FootballField represents a location where football matches can be played.

Fields can be created by users and reused for future matches.

The same football field may host thousands of matches over time.

---

# Entity Definition

| Field | Type | Required | Description |
|--------|------|----------|-------------|
| Id | UUID | Yes | Primary Key |
| Name | String | Yes | Name of the football field |
| Address | String | Yes | Full address |
| Latitude | Decimal | Yes | GPS Latitude |
| Longitude | Decimal | Yes | GPS Longitude |
| Capacity | Integer | Yes | Maximum supported players |
| MatchDurationMinutes | Integer | Yes | Usually 60, 90 or 120 |
| Price | Decimal | Yes | Full match price |
| ReservationFee | Decimal | Yes | Reservation amount paid in advance |
| Surface | Enum | Yes | Artificial Turf, Natural Grass, Concrete |
| Indoor | Boolean | Yes | Indoor or outdoor |
| Parking | Boolean | Yes | Parking available |
| Shower | Boolean | Yes | Shower available |
| Lights | Boolean | Yes | Night lighting available |
| PhoneNumber | String | No | Contact number |
| GoogleMapsUrl | String | No | Optional |
| Notes | String | No | Additional information |
| CreatedBy | UUID | Yes | User who created the field |
| CreatedAt | DateTime | Yes | Creation date |
| UpdatedAt | DateTime | Yes | Last modification |

---

# Business Rules

## Ownership

The creator of the field becomes its owner.

Ownership may be transferred in a future version.

---

## Editing

Only the owner may edit field information.

Administrators may edit any field.

---

## Visibility

Every field is public.

Any organizer may use an existing field when creating a match.

---

## Duplicate Prevention

Before creating a new field, the application should search for existing nearby fields with similar names.

The user may choose one of the existing fields instead.

---

## Price Changes

Changing the field price affects only future matches.

Existing matches keep the original price.

---

## Reservation Fee Changes

Changing the reservation fee affects only future matches.

---

## Match Duration

Every field defines its default match duration.

Matches inherit this value automatically.

---

## Capacity

Capacity determines the maximum number of participants.

Typical values:

- 10
- 12
- 14
- 16
- 22

---

# Future Extensions

The entity is designed to support:

- Photos
- Ratings
- Reviews
- Opening hours
- Booking directly through Kawwer
- Verified field owners
- Multiple phone numbers
- Multiple prices