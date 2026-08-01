# Requirements traceability

This matrix compares the stated project requirements with the current code. “Implemented” means a working path exists in the repository; it does not imply full production hardening or completed test evidence.

## Functional requirements

| ID | Requirement | Status | Implementation evidence / boundary |
| --- | --- | --- | --- |
| FR1 | User registration | Implemented | `AccountController.Register`; Organizer/Vendor choice; Gmail validation |
| FR2 | User login | Implemented | Identity cookie login; confirmed-email enforcement |
| FR3 | Role management | Implemented for access; limited administration | Seeded Admin/Organizer/Vendor roles and authorization attributes; no admin role-management UI |
| FR4 | Event management | Partially implemented | Guided organizer create/edit, deadlines, attendance, contacts, facilities, vendor terms, cover image, JPG/PNG/WEBP/PDF stall layout upload, and ownership checks; no delete/archive action |
| FR5 | Stall management | Partially implemented | Organizer batch generation from category/count/prefix, derived Basic/Standard/Premium sizes, edit/status fields and duplicate-number validation; no delete action |
| FR6 | Event browsing | Implemented | Public list, text search, exact category filter, upcoming display in vendor dashboard |
| FR7 | Stall viewing | Implemented | Public event detail, stall cards, status and layout coordinates/map |
| FR8 | Booking request | Implemented | Vendor-only request, validation, active-request check and atomic stall claim |
| FR9 | Booking review | Implemented | Organizer-owned review with vendor profile context, approve/reject, competing-request rejection |
| FR10 | Payment reference | Implemented | Vendor submission and organizer status review; no payment processing |
| FR11 | Notification | Implemented | Persistent in-app booking notifications with hover preview, grouped/paged notification center, approval email; no real-time push |
| FR12 | Dashboard | Implemented | Admin, Organizer and Vendor views |

## Non-functional requirements

| ID | Requirement | Status | Implementation evidence / remaining proof |
| --- | --- | --- | --- |
| NFR1 | Usability | Implemented structurally | Role navigation, dashboards, clear actions/statuses; requires user-acceptance results for evaluation |
| NFR2 | Accessibility | Partially evidenced | Browser-based Razor/HTML interface and responsive layout; no documented WCAG audit |
| NFR3 | Security | Implemented with hardening needed | Identity, roles, ownership, anti-forgery; committed secrets must be removed/rotated and upload controls should be standardized |
| NFR4 | Data consistency | Implemented in core path | Transactions and conditional updates protect request/approval; provider-specific concurrency tests still required |
| NFR5 | Maintainability | Implemented structurally | MVC folders, DbContext, services and view models; migrations and extracted booking service would improve evolution |
| NFR6 | Responsiveness | Implemented structurally | Bootstrap/custom responsive CSS; device/browser test evidence should be maintained |
| NFR7 | Reliability | Partially evidenced | Validation and guarded workflows exist; SMTP remains synchronous and no automated test suite is visible in the project |

## Module-to-code map

| Module | Controllers | Models/data | Views |
| --- | --- | --- | --- |
| Account | `AccountController` | `ApplicationUser`, account view models, Identity tables | `Views/Account/*` |
| Event | `EventsController` | `Event`, `EventFormViewModel` | `Views/Events/*` |
| Stall | `StallsController` | `Stall`, `StallFormViewModel` | `Views/Stalls/Form.cshtml`, event detail |
| Booking/payment | `BookingsController` | `Booking`, status enums | `Views/Bookings/Review.cshtml`, dashboards, event detail |
| Dashboard | `DashboardController` | Dashboard view models | `Views/Dashboard/*` |
| Notification | `NotificationsController`; booking actions create records | `Notification` | `Views/Notifications/Index.cshtml` |
| Contact/email | `HomeController`, account/booking actions | `IEmailSender`, `ConsoleEmailSender` | Contact and account pages |

## Verification checklist

### Functional

- Register both Vendor and Organizer accounts and verify email-confirmation gating.
- Verify password reset, password change, profile updates, and logout.
- Create/edit an event and upload accepted/rejected event cover and stall layout file types and sizes.
- Generate/edit stalls and verify generated duplicate numbers are rejected within one event.
- Search events by name/venue/description and filter by category.
- Request, pay, review, approve, reject, and cancel bookings.
- Verify notification ownership, hover preview, grouping, paging, and read-state changes.
- Verify each dashboard returns only the intended role's data.

### Authorization

- Attempt each restricted route while anonymous and with every wrong role.
- Attempt organizer actions using IDs owned by another organizer.
- Attempt vendor payment/cancel actions using another vendor's booking ID.
- Confirm state-changing requests without valid anti-forgery tokens are rejected.

### Concurrency

- Submit simultaneous requests for one available stall and verify only one atomic claim succeeds.
- Attempt simultaneous organizer approvals and verify only one booking becomes approved/booked.
- Modify stall state between review and approval and verify the stale approval does not overwrite it.
- Run these tests against both SQLite development fallback and SQL Server target storage.

### Operational

- Start with SQL Server available and unavailable in Development to verify provider selection.
- Start in non-development with valid SQL Server settings and verify HTTPS/HSTS behavior.
- Verify SMTP success and failure paths without exposing credentials in logs.
- Verify uploaded media and data-protection keys survive deployment restarts where required.

## Highest-priority gaps before production

1. Remove and rotate committed SMTP credentials; use a secret store.
2. Add versioned EF Core migrations and deployment migration procedures.
3. Add automated integration tests for role ownership and concurrent booking approval.
4. Move email delivery to a queue/provider with retry and observability.
5. Replace local image/key storage for multi-instance or ephemeral hosting.
6. Add database indexes/constraints that reinforce controller-level invariants.
