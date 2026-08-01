# Process flows

## 1. Registration and sign-in

```mermaid
flowchart TD
    start([Open registration]) --> enter[Enter name, Gmail, password, and role]
    enter --> valid{Form and password valid?}
    valid -->|No| errors[Show validation errors]
    errors --> enter
    valid -->|Yes| create[Create unconfirmed Identity user]
    create --> created{Identity creation succeeded?}
    created -->|No| identityErrors[Show Identity errors]
    identityErrors --> enter
    created -->|Yes| role[Assign Organizer or Vendor role]
    role --> email[Send verification link]
    email --> confirm[User opens confirmation link]
    confirm --> confirmed{Token accepted?}
    confirmed -->|No| retry[Resend verification]
    retry --> email
    confirmed -->|Yes| login[Submit Gmail and password]
    login --> allowed{Credentials valid and email confirmed?}
    allowed -->|No| loginError[Show login guidance]
    loginError --> login
    allowed -->|Yes| dashboard[Redirect to role dashboard]
```

Notes:

- Self-registration allows only Organizer or Vendor; any other posted role value becomes Vendor.
- Account form validation accepts Gmail addresses only.
- Development can expose generated verification/reset links through temporary UI data for testing.

## 2. Organizer event and stall setup

```mermaid
flowchart LR
    start([Organizer dashboard]) --> eventForm[Create event]
    eventForm --> validateEvent{Profile, terms, dates, and uploads valid?}
    validateEvent -->|No| eventErrors[Show form errors]
    eventErrors --> eventForm
    validateEvent -->|Yes| saveEvent[Save owned event, cover, and layout]
    saveEvent --> stallForm[Continue to stall batch generator]
    stallForm --> ownEvent{Organizer owns event?}
    ownEvent -->|No| notFound[Return not found]
    ownEvent -->|Yes| generate[Generate numbers from prefix, start, and quantity]
    generate --> derive[Derive stall size from Basic, Standard, or Premium category]
    derive --> unique{Generated stall numbers unique in event?}
    unique -->|No| stallErrors[Show form errors]
    stallErrors --> stallForm
    unique -->|Yes| saveStalls[Save generated stalls, price, status, and grid positions]
    saveStalls --> details[Open event details]
```

Event profiles include an application deadline, expected attendance, vendor contact details, included facilities, vendor requirements, and cancellation terms. Event covers allow JPG, PNG, or WEBP up to 5 MB. Stall layout uploads allow JPG, PNG, WEBP, or PDF up to 5 MB. Stall creation no longer requires manual length and breadth entry; those values come from the selected category.

## 3. Vendor booking and deposit workflow

```mermaid
flowchart TD
    browse([Browse events]) --> details[Open event and stall map]
    details --> choose[Choose a stall and submit note]
    choose --> eligible{Vendor, event and application window open, note valid, stall available?}
    eligible -->|No| rejectInput[Return with an error]
    eligible -->|Yes| duplicate{Vendor already has an active request?}
    duplicate -->|Yes| rejectInput
    duplicate -->|No| claim[Atomically set Available to Pending]
    claim --> claimed{One stall row updated?}
    claimed -->|No| raceLost[Report that another vendor was first]
    claimed -->|Yes| booking[Create pending booking]
    booking --> organizerNotice[Notify organizer]
    organizerNotice --> vendorDashboard[Open vendor dashboard]
    vendorDashboard --> payment[Submit deposit reference]
    payment --> paymentValid{Booking pending and reference valid?}
    paymentValid -->|No| paymentError[Keep current booking state]
    paymentValid -->|Yes| submitted[Set payment status to Submitted]
    submitted --> paymentNotice[Notify organizer]
    paymentNotice --> review[Organizer reviews request with vendor profile context]
```

## 4. Organizer review and concurrency-safe approval

```mermaid
flowchart TD
    review([Open owned booking review]) --> pending{Booking still Pending?}
    pending -->|No| alreadyReviewed[Stop: already reviewed]
    pending -->|Yes| deposit{Payment Submitted or Verified?}
    deposit -->|No| needsDeposit[Stop: request deposit reference]
    deposit -->|Yes| decision{Organizer decision}

    decision -->|Reject| reject[Set booking Rejected]
    reject --> release{Stall currently Pending?}
    release -->|Yes| available[Set stall Available]
    release -->|No| rejectNotice[Keep stall state]
    available --> rejectNotice[Notify vendor]

    decision -->|Approve| claim[Atomically set Available or Pending to Booked]
    claim --> claimed{One stall row updated?}
    claimed -->|No| autoReject[Reject request because stall changed]
    claimed -->|Yes| approve[Set booking Approved and payment Verified]
    approve --> competitors[Reject competing pending requests]
    competitors --> save[Save transaction]
    save --> concurrency{Concurrency exception?}
    concurrency -->|Yes| rollback[Roll back and show latest-state warning]
    concurrency -->|No| approveNotice[Notify and email vendor]
```

The approval guard is implemented as a conditional database update rather than a read-then-write sequence. This is the primary protection against duplicate confirmation.

## 5. Vendor cancellation

```mermaid
flowchart LR
    request([Cancel own booking]) --> pending{Booking Pending?}
    pending -->|No| deny[Reject cancellation]
    pending -->|Yes| cancel[Set booking Cancelled]
    cancel --> release{Stall Pending?}
    release -->|Yes| available[Set stall Available]
    release -->|No| preserve[Preserve stall status]
    available --> notify[Notify organizer]
    preserve --> notify
    notify --> dashboard[Return to vendor dashboard]
```

## 6. Status transition rules

### Stall

| From | To | Trigger |
| --- | --- | --- |
| `Available` | `Pending` | First successful vendor request claim |
| `Pending` | `Booked` | Organizer successfully approves a deposited request |
| `Pending` | `Available` | Pending request is rejected or cancelled |
| `Available` or `Pending` | `Booked` | Guarded approval update |
| Any | Organizer-selected status | Organizer edits the stall form |

### Booking

| From | To | Trigger |
| --- | --- | --- |
| New | `Pending` | Successful vendor request |
| `Pending` | `Approved` | Organizer approval after deposit submission |
| `Pending` | `Rejected` | Organizer rejection, conflicting approval, or lost availability |
| `Pending` | `Cancelled` | Vendor cancellation |

### Payment

| From | To | Trigger |
| --- | --- | --- |
| `NotSubmitted` | `Submitted` | Vendor supplies a valid reference |
| `Submitted` | `Verified` | Organizer verifies it or approves the booking |
| `Submitted` or `Verified` | `Rejected` | Organizer rejects payment while booking remains pending |

The model enum permits transitions beyond this table, but controller actions enforce the flows shown above.
