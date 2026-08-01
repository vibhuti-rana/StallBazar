# Project overview

## Purpose

StallBazar replaces phone calls, messages, spreadsheets, and paper-based stall allocation with a browser-based workflow. Its central consistency rule is that a stall can move from available to pending to booked through guarded database updates, preventing two vendors from being confirmed for the same stall.

## Primary actors

| Actor | Main goals | Current capabilities |
| --- | --- | --- |
| Public visitor | Discover relevant events | View marketing pages, search/filter events, open event and stall details, register or sign in |
| Vendor | Secure a suitable stall | Browse events, request an available stall, add a vendor note, submit a deposit reference, cancel a pending request, view booking statuses and notifications |
| Organizer | Publish and operate events | Create/edit owned events, upload event/map images, create/edit owned stalls, review deposit status, approve/reject owned-event bookings, receive notifications |
| Administrator | Monitor the platform | View aggregate users/events/stalls/bookings and recent users/bookings |

## Core business modules

| Module | Responsibility | Main implementation |
| --- | --- | --- |
| Account | Registration, Gmail verification, login, password reset/change, profile, logout | `AccountController`, ASP.NET Core Identity |
| Event discovery | Public search, category filter, event details and stall map | `EventsController.Index`, `EventsController.Details` |
| Event management | Organizer-owned event creation/editing and image upload | `EventsController.Create/Edit` |
| Stall management | Organizer-owned stall creation/editing, layout coordinates and status | `StallsController` |
| Booking | Request, payment reference, cancel, review, approve and reject | `BookingsController` |
| Dashboard | Role-specific operational views | `DashboardController` |
| Notification | In-app booking activity feed | `NotificationsController`, `Notification` entity |
| Email | Account links and approval email delivery | `IEmailSender`, `ConsoleEmailSender` |

## Implemented scope

- Server-rendered responsive web application.
- Registration as Organizer or Vendor; Admin accounts are seeded, not self-selected.
- Gmail-only account validation and confirmed-email requirement.
- Role-based and ownership-based authorization.
- Public event search by free text and exact category.
- Event configuration with application deadlines, expected attendance, vendor contact details, facilities, requirements, cancellation terms, and uploaded event/map images.
- Visual stall positioning through `PositionX` and `PositionY`.
- One active vendor request per stall and vendor.
- Atomic claim of an available stall when a request is created.
- Deposit reference submission and organizer-side payment-status review.
- Guarded approval, automatic rejection of competing pending requests, and vendor notification.
- SQLite development fallback and SQL Server target storage.

## Explicitly out of scope

- Online payment processing; the system stores references and verification status only.
- Native mobile applications.
- Multi-tenant enterprise isolation.
- SMS, social media, or automated marketing integrations.
- Advanced analytics, business intelligence, or accounting.
- 3D/CAD venue modelling.

## Important domain rules

1. Only an Organizer can create events and stalls.
2. An Organizer can edit or review only records belonging to events they own.
3. Only a Vendor can create, pay for, or cancel their own booking request.
4. A request is accepted only while the event and vendor application window are open and the stall is available.
5. Creating a request atomically changes the stall from `Available` to `Pending`.
6. A booking cannot be approved until a deposit reference is submitted or verified.
7. Approval atomically changes the stall to `Booked` and rejects competing pending requests.
8. Rejection or cancellation releases a `Pending` stall back to `Available`.
9. Payment details can change only while a booking is pending and unverified.
10. Opening the notification inbox marks all unread notifications as read.

## Current implementation boundaries

- Event and stall delete operations are not implemented.
- The Admin dashboard is observational; user, role, event, or booking administration is not implemented.
- Notifications are persisted and displayed but are not pushed in real time.
- Email delivery uses synchronous SMTP calls inside web requests; there is no queue or retry worker.
- Uploaded images use local web-root storage; there is no object-storage provider or cleanup workflow.
- Database creation uses `EnsureCreated` plus SQL Server-specific schema guards rather than versioned EF Core migrations.

## Quality attributes

| Attribute | Current mechanism |
| --- | --- |
| Security | Identity password policy, confirmed email, authorization attributes, ownership filters, anti-forgery validation |
| Consistency | Transactions, conditional `ExecuteUpdateAsync`, stall status checks, and a row-version field |
| Maintainability | MVC separation across controllers, models, views, data, and services |
| Portability | SQL Server production provider with SQLite development fallback |
| Usability | Role-specific navigation, dashboards, status labels, forms, and public event discovery |
| Responsiveness | Bootstrap assets plus custom responsive CSS and Razor layouts |
