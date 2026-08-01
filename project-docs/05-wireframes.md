# Wireframes

These low-fidelity wireframes reflect the current Razor views. They describe information hierarchy and actions; colors, typography, and exact spacing are controlled by the existing CSS.

## Global desktop shell

```text
+--------------------------------------------------------------------------------+
| StallBazar | role-aware navigation/search | notifications | settings | profile |
+--------------------------------------------------------------------------------+
|                                                                                |
|                                PAGE CONTENT                                    |
|                                                                                |
+--------------------------------------------------------------------------------+
| Product links | Event types | Company | Account                                |
+--------------------------------------------------------------------------------+
```

Navigation changes by context:

- Public: Browse Events, How it works, For Organizers, Support, Sign in, Sign up.
- Vendor: Vendor Home, Browse Events, search, notifications, settings, profile, logout.
- Organizer: Organizer Home, Create Event, All Events, notifications, settings, profile, logout.
- Admin: Admin Home, Events, notifications, settings, profile, logout.

## Public landing page

```text
+--------------------------------------------------------------------------------+
| HERO: value proposition                         | event/market visual           |
| [Browse events] [Start as vendor]               |                               |
+--------------------------------------------------------------------------------+
| Brand/category strip                                                            |
+--------------------------------------------------------------------------------+
| Product overview: Publish | Request | Review payments | Track updates           |
+--------------------------------------------------------------------------------+
| Booking flow feature cards                                                      |
+--------------------------------------------------------------------------------+
| KPI band and role stories                                                       |
+--------------------------------------------------------------------------------+
| Vendor | Organizer | Admin role showcase                                        |
+--------------------------------------------------------------------------------+
| Marketplace feature cards and event stories                                    |
+--------------------------------------------------------------------------------+
| How it works | articles | FAQ                                                   |
+--------------------------------------------------------------------------------+
| Final registration call to action                                               |
+--------------------------------------------------------------------------------+
```

## Event discovery

```text
+--------------------------------------------------------------------------------+
| Browse hero: find opportunities by category, venue, price, and map              |
+--------------------------------------------------------------------------------+
| Upcoming stall opportunities                         [Create event: Organizer]   |
| [Search text................................] [Category v] [Search]              |
| [All] [Food] [Concert] [Fashion] [Books] [...]                                  |
+--------------------------------------------------------------------------------+
| [Event image]  Event name                         [Event image]  Event name      |
|                venue / date / price                              venue / date    |
|                available stall count [View stalls]               [View stalls]  |
+--------------------------------------------------------------------------------+
| Additional event cards...                                                       |
+--------------------------------------------------------------------------------+
```

Mobile behavior: the search controls stack, category pills scroll/wrap, and cards become a single column.

## Event detail and stall selection

```text
+--------------------------------------------------------------------------------+
| Event hero / image | Event title, category, date [Edit] [Generate stalls: owner]|
+--------------------------------------------------------------------------------+
| Venue, description, facilities, terms     | Dates, deadline and organizer       |
+--------------------------------------------------------------------------------+
| Stall layout image or PDF / hint                                              |
+--------------------------------------------------------------------------------+
| Visual stall grid: A01 Available | A02 Pending | A03 Booked | ...               |
+--------------------------------------------------------------------------------+
| Stall card: number, name, category, type, zone, derived size, price, status     |
|   Organizer owner: [Edit]                                                       |
|   Vendor + available: [Vendor note........................] [Request stall]      |
|   Anonymous: [Login to request]                                                 |
+--------------------------------------------------------------------------------+
```

## Vendor dashboard

```text
+--------------------------------------------------------------------------------+
| Discover the right event for your stall                         [Browse events] |
+--------------------------------------------------------------------------------+
| [Food events] [Concert events] [Fashion bazaars] [Book fairs]                  |
+--------------------------------------------------------------------------------+
| Upcoming events                         | Your bookings                         |
| Event card [View map]                   | Event / stall / statuses             |
| Event card [View map]                   | [Payment reference....] [Submit]      |
|                                         | [Cancel pending request]              |
+--------------------------------------------------------------------------------+
| Near events                             | Coming soon                           |
| compact list cards                      | compact list cards                    |
+--------------------------------------------------------------------------------+
```

## Organizer dashboard

```text
+--------------------------------------------------------------------------------+
| Manage events, maps, photos, and requests                       [Create event]  |
+--------------------------------------------------------------------------------+
| Guidance/summary panel                                                         |
+--------------------------------------------------------------------------------+
| Your events                              | Pending requests                     |
| Event / date / stall count               | Vendor photo/name/profile preview     |
| [Open] [Generate stalls]                 | Stall - Event / payment status [Review]|
| ...                                      | ...                                  |
+--------------------------------------------------------------------------------+
| Make listings visual                     | Keep operations clear                |
+--------------------------------------------------------------------------------+
```

## Booking review

```text
+--------------------------------------------------------------------------+
| Event name - Stall number                                                |
+--------------------------------------------------------------------------+
| Vendor profile card: photo, name, business, email, phone, city, note      |
+--------------------------------------------------------------------------+
| Stall/payment summary: category, zone, size, price, deposit, reference    |
+--------------------------------------------------------------------------+
| Payment review: [Submitted/Verified/Rejected v] [Update]                 |
+--------------------------------------------------------------------------+
| Approval note [.......................................................]  |
| [Approve booking]                                                        |
+--------------------------------------------------------------------------+
| Rejection note [......................................................]  |
| [Reject and release stall]                                               |
+--------------------------------------------------------------------------+
```

## Admin dashboard

```text
+--------------------------------------------------------------------------------+
| System overview                                                               |
+--------------------------------------------------------------------------------+
| [Users count] [Events count] [Stalls count] [Bookings count]                   |
+--------------------------------------------------------------------------------+
| Recent bookings                          | Recent users/organizer details        |
| event / stall / vendor / status          | name / email / profile information   |
+--------------------------------------------------------------------------------+
```

## Event and stall forms

```text
+--------------------------------------------------------------------------+
| Create/Edit event and generated stall inventory                          |
+--------------------------------------------------------------------------+
| EVENT                                                                    |
| Name | Venue | Description | Expected visitors                           |
| Category | Starting price | Start | End | Application deadline                  |
| Vendor email/phone | Facilities | Requirements | Cancellation policy            |
| Event image upload | Stall layout upload (JPG/PNG/WEBP/PDF) | Map hint    |
| [Save event and add stalls] [Cancel]                                     |
+--------------------------------------------------------------------------+
| STALL CREATE                                                             |
| Quantity | Number prefix | Start from                                    |
| Category cards: Basic 2m x 2m | Standard 3m x 3m | Premium 4m x 4m       |
| Optional base name | Type | Zone | Price | Status                        |
| Start column | Start row                                                |
| [Generate stalls] [Cancel]                                               |
+--------------------------------------------------------------------------+
| STALL EDIT                                                               |
| Number | Name | Category | Type | Zone | Price | Status                 |
| Start column | Start row | derived size preview                         |
| [Save stall] [Cancel]                                                    |
+--------------------------------------------------------------------------+
```

## Notifications

```text
+--------------------------------------------------------------------------+
| Bell icon hover/focus preview: latest five notifications + unread count   |
+--------------------------------------------------------------------------+
| Notification center mobile-style feed                                     |
| Today                                                                     |
|   Notification title | time | message | open                              |
| Yesterday                                                                 |
|   Notification title | yesterday/time | message | open                    |
| Older date heading                                                        |
|   Notification title | full date/time | message | open                    |
| [Load more notifications]                                                 |
+--------------------------------------------------------------------------+
```

## Account screens

```text
+--------------------------------------------------------------------------------+
| Context/brand visual                    | Authentication card                    |
| Short product message                  | Login: Gmail / password / remember     |
|                                        | [Sign in]                              |
|                                        | Forgot | Resend verification | Register|
+--------------------------------------------------------------------------------+

+--------------------------------------------------------------------------------+
| Profile form: name, email, phone,       | Live profile preview                   |
| business, city, bio, image [Save]       | avatar / name / organization / bio     |
+--------------------------------------------------------------------------------+

+--------------------------------------------------------------------------+
| Settings: change password form | logout                                  |
+--------------------------------------------------------------------------+
```

## Responsive priority

On narrow screens, preserve this order:

1. Page title and primary action.
2. Status or validation message.
3. Main task form/list.
4. Supporting facts and secondary content.
5. Footer navigation.

Destructive or state-changing actions should remain clearly separated from navigation links, with confirmation retained for booking cancellation and rejection.
