# Document Information

**Status:** Approved

**Version:** 1.0

**Last Updated:** 2026-06-30

**Owner:** Kawwer Team

---

# Design System

## Purpose

The Design System ensures every screen in Kawwer has a consistent appearance and behavior.

All future UI development must follow this document.

---

# Design Principles

- Fast
- Clean
- Modern
- Mobile-first
- Accessible
- Consistent

The application should feel closer to WhatsApp and Google Maps than to a complicated enterprise application.

---

# Theme

Version 1 supports:

- Light Theme
- Dark Theme

Theme changes instantly.

---

# Primary Color

Football Green

```
#2E7D32
```

---

# Secondary Color

Blue

```
#1976D2
```

---

# Success

```
#4CAF50
```

---

# Warning

```
#FB8C00
```

---

# Error

```
#E53935
```

---

# Background

Light

```
#F5F5F5
```

Dark

```
#121212
```

---

# Surface

Light

```
#FFFFFF
```

Dark

```
#1E1E1E
```

---

# Typography

Primary Font

Inter

Fallback

System Default

---

# Font Sizes

Display

32

Heading

24

Title

20

Body

16

Caption

14

Small

12

---

# Border Radius

Cards

16

Buttons

12

Dialogs

20

Input Fields

12

---

# Spacing

Tiny

4

Small

8

Medium

16

Large

24

Extra Large

32

---

# Buttons

Primary

Filled

Secondary

Outlined

Danger

Red Filled

Text

Borderless

Floating Action Button

Circular

Contains:

+

Create Match

---

# Cards

Cards are used for:

- Match
- Player
- Group
- Football Field
- Statistics

Every card includes:

- Rounded corners
- Shadow
- Padding
- Touch feedback

---

# Icons

Use:

Material Symbols

Filled style.

---

# Badges

Used for:

- Reliability
- Waiting List
- Payment Status
- Match Confidence

---

# Avatars

Circular.

Fallback:

User initials.

---

# Lists

Support:

- Swipe Actions
- Pull to Refresh

---

# Input Fields

Support:

- Labels
- Validation
- Helper Text
- Error Messages

---

# Loading

Use skeleton placeholders.

Avoid blocking loading indicators whenever possible.

---

# Empty States

Every empty page displays:

- Illustration
- Title
- Description
- Primary Action

---

# Animations

Use subtle animations for:

- Navigation
- Buttons
- Loading
- Success

Animation duration:

150–250 ms

---

# Accessibility

Support:

- Screen readers
- Large text
- High contrast
- Minimum touch target: 48x48 dp

---

# Localization

Supported languages:

- English
- French
- Arabic

Arabic uses RTL layout.

Currency:

TND

Date and time formatting follow the selected language.

---

# Acceptance Criteria

- Every screen follows the same spacing.
- Colors remain consistent.
- Buttons use standard styles.
- Typography follows the design system.
- Dark mode is fully supported.
- Accessibility requirements are met.