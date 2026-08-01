# Access control and routes

## Authorization model

Access is enforced in two layers:

1. Role authorization through `[Authorize]` and `[Authorize(Roles = ...)]`.
2. Record ownership through database predicates using the authenticated user ID.

The UI hides irrelevant actions, but controller enforcement is the security boundary.

## Capability matrix

| Capability | Public | Vendor | Organizer | Admin |
| --- | :---: | :---: | :---: | :---: |
| View home/about/privacy/contact | Yes | Yes | Yes | Yes |
| Search events and view details | Yes | Yes | Yes | Yes |
| Register/login/reset/verify | Yes | N/A while signed in | N/A while signed in | N/A while signed in |
| View own profile/settings/notifications | No | Yes | Yes | Yes |
| View vendor dashboard | No | Yes | No | No |
| Request/cancel own booking | No | Yes | No | No |
| Submit own payment reference | No | Yes | No | No |
| View organizer dashboard | No | No | Yes | No |
| Create event | No | No | Yes | No |
| Edit event | No | No | Owned only | No |
| Create/edit stall | No | No | Owned event only | No |
| Review/approve/reject booking | No | No | Owned event only | No |
| Update payment-review status | No | No | Owned event only | No |
| View admin dashboard | No | No | No | Yes |

## Route inventory

Routes use the conventional pattern `/Controller/Action/{id?}` unless query/form parameters are shown.

### HomeController

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| GET | `/Home/Index` or `/` | Public | Landing page |
| GET | `/Home/About` | Public | Product/process information |
| GET | `/Home/Privacy` | Public | Privacy page |
| GET | `/Home/Contact` | Public | Contact form |
| POST | `/Home/Contact` | Public + anti-forgery | Send support email |
| GET | `/Home/Error` | Public | Error response page |

### AccountController

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| GET/POST | `/Account/Login` | Public | Cookie sign-in |
| GET/POST | `/Account/Register` | Public | Organizer/Vendor registration |
| GET/POST | `/Account/ForgotPassword` | Public | Issue password reset link |
| GET/POST | `/Account/ResendVerification` | Public | Reissue confirmation link |
| GET | `/Account/ConfirmEmail` | Public with token | Confirm email |
| GET/POST | `/Account/ResetPassword` | Public with token | Set replacement password |
| GET/POST | `/Account/Profile` | Authenticated | View/update own profile |
| GET | `/Account/Settings` | Authenticated | Security/preferences page |
| POST | `/Account/ChangePassword` | Authenticated + anti-forgery | Change own password |
| POST | `/Account/Logout` | Authenticated + anti-forgery | End cookie session |
| GET | `/Account/AccessDenied` | Public | Forbidden-access page |

### EventsController

| Method | Route | Access | Ownership/purpose |
| --- | --- | --- | --- |
| GET | `/Events/Index?q=&category=` | Public | Search and filter events |
| GET | `/Events/Details/{id}` | Public | Event, organizer and stall layout |
| GET/POST | `/Events/Create` | Organizer | Create an owned event |
| GET/POST | `/Events/Edit/{id}` | Organizer | Owned event only |

### StallsController

The entire controller requires the Organizer role.

| Method | Route | Ownership/purpose |
| --- | --- | --- |
| GET | `/Stalls/Create?eventId=` | Event must belong to organizer |
| POST | `/Stalls/Create` | Event must belong to organizer; number unique within event |
| GET | `/Stalls/Edit/{id}` | Stall's event must belong to organizer |
| POST | `/Stalls/Edit` | Stall's event must belong to organizer |

### BookingsController

The controller requires authentication; each action further restricts role and ownership.

| Method | Route | Access | Ownership/purpose |
| --- | --- | --- | --- |
| POST | `/Bookings/Create` | Vendor | Create request while event/application window is open |
| GET | `/Bookings/Review/{id}` | Organizer | Booking's event must be owned |
| POST | `/Bookings/Approve/{id}` | Organizer | Owned booking; guarded approval |
| POST | `/Bookings/Reject/{id}` | Organizer | Owned pending booking |
| POST | `/Bookings/UpdatePayment/{id}` | Vendor | Own pending booking |
| POST | `/Bookings/UpdatePaymentStatus/{id}` | Organizer | Owned pending booking |
| POST | `/Bookings/Cancel/{id}` | Vendor | Own pending booking |

All booking POST actions use anti-forgery validation.

### DashboardController

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| GET | `/Dashboard/Index` | Authenticated | Redirect by role: Admin, Organizer, otherwise Vendor |
| GET | `/Dashboard/Admin` | Admin | Counts and recent platform activity |
| GET | `/Dashboard/Organizer` | Organizer | Owned events and pending requests |
| GET | `/Dashboard/Vendor` | Vendor | Discovery lists and own bookings |

### NotificationsController

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| GET | `/Notifications/Index` | Authenticated | List own notifications and mark unread rows read |

## Security controls

- State-changing form actions use anti-forgery tokens.
- Posted IDs are re-queried with owner predicates before modification.
- Login redirects accept only local return URLs.
- Contact-form values are HTML encoded before inclusion in email HTML.
- Event image uploads validate size, MIME type, and extension; randomized names prevent direct filename reuse.
- Profile images use randomized names and extension validation.

## Access-control caveats

- `Dashboard.Index` sends any authenticated user who is neither Admin nor Organizer to the Vendor action; the Vendor action itself still requires the Vendor role, so an unrecognized-role user receives access denied.
- Admin is intentionally not a superuser for organizer/vendor controller actions. It can view only the dedicated Admin dashboard and public/authenticated common pages.
- Returning `NotFound` for records outside the current user's ownership is deliberate and avoids confirming that the record exists.
