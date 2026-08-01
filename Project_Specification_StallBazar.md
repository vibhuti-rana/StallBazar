# Project Specification Form

## Project Title

Web-Based Stall Booking and Management System (StallBazar)

## Student Details

Student Name: Vibhuti RL Rana

Student ID: NP069778

Programme: BSc (Hons) in Information Technology

Institution: Asia Pacific University of Technology and Innovation (APU)

Supervisor: Sujan Khyaju

Related SDG: SDG 9 - Industry, Innovation and Infrastructure

Secondary SDG Alignment: SDG 8 - Decent Work and Economic Growth; SDG 11 - Sustainable Cities and Communities

## Project Background

Small and medium-scale event organizers commonly manage stall booking through phone calls, messaging applications, spreadsheets and paper records. These methods are easy to start with, but they become difficult to control when the number of vendors, stall categories, prices and payments increases. Manual coordination can cause double booking, unclear stall availability, poor communication, payment tracking problems and weak record keeping.

StallBazar is proposed as a web-based stall booking and management system for small-scale event organizers and vendors. The system will allow organizers to create events, define stall details, manage booking requests and approve or reject vendor requests. Vendors will be able to browse events, view stall details, request available stalls and track the status of their bookings. The system is intended to provide a practical and browser-based solution for digitizing stall booking workflows.

## Problem Statement

The main problem addressed by this project is the lack of an integrated, reliable and accessible stall booking system for small-scale event organizers and vendors. Existing manual and semi-digital methods are fragmented and can result in the following issues:

- Unclear stall availability.
- Duplicate or conflicting stall bookings.
- Slow communication between organizers and vendors.
- Inconsistent payment reference tracking.
- Difficulty monitoring pending, approved, rejected and booked requests.
- Lack of role-based control between organizers and vendors.

Enterprise event management platforms exist, but they are often too broad, expensive or complex for small local events. A lightweight system focused specifically on stall booking is therefore required.

## Project Aim

The aim of this project is to design, develop and test a web-based stall booking and management system that improves booking accuracy, operational efficiency and user experience for small-scale event organizers and vendors through role-based access control and concurrency-safe booking confirmation.

## Project Objectives

The objectives of this project are:

1. To design and develop a browser-based stall booking and management system using ASP.NET, HTML, CSS, JavaScript and SQL Server.
2. To allow organizers to create events, configure stalls, manage stall availability and review vendor booking requests.
3. To allow vendors to browse events, view stall details, request stalls and track booking status.
4. To implement role-based access control so that organizer, vendor and admin functions are properly separated.
5. To implement optimistic concurrency control to reduce the risk of duplicate stall confirmation.
6. To support booking status and payment reference tracking for organizer review.
7. To evaluate the system through functional testing, role-based access testing, concurrency testing and user acceptance feedback.

## Scope of the Project

### In Scope

- User registration and login.
- Email verification and password reset support.
- Role-based access for Admin, Organizer and Vendor.
- Event creation and event listing.
- Stall configuration including number, name, type, tier, size, price, zone and status.
- Visual stall layout display.
- Vendor stall booking request workflow.
- Organizer approval and rejection workflow.
- Payment reference and payment status tracking.
- Notifications for booking-related activity.
- Dashboard views for different user roles.
- Functional, access-control and concurrency-related testing.

### Out of Scope

- Online payment gateway integration.
- Dedicated mobile application development.
- Enterprise-scale multi-tenant deployment.
- Advanced analytics and business intelligence reporting.
- SMS gateway, social media integration or automated marketing tools.
- 3D venue modelling or CAD-based layout management.

## Proposed Users

### Admin

The Admin user monitors the overall platform, reviews dashboard information and supports system-level management.

### Organizer

The Organizer creates events, configures stalls, reviews vendor booking requests, verifies payment references and approves or rejects stall requests.

### Vendor

The Vendor browses events, reviews stall information, sends booking requests, submits payment references and tracks booking status.

## Functional Requirements

| ID | Requirement | Description |
| --- | --- | --- |
| FR1 | User Registration | The system shall allow users to register as organizers or vendors. |
| FR2 | User Login | The system shall authenticate registered users using email and password. |
| FR3 | Role Management | The system shall restrict features based on Admin, Organizer and Vendor roles. |
| FR4 | Event Management | Organizers shall be able to create and edit event details. |
| FR5 | Stall Management | Organizers shall be able to create and update stalls for their events. |
| FR6 | Event Browsing | Vendors and public users shall be able to browse listed events. |
| FR7 | Stall Viewing | Vendors shall be able to view stall details and availability. |
| FR8 | Booking Request | Vendors shall be able to request an available stall. |
| FR9 | Booking Review | Organizers shall be able to approve or reject vendor booking requests. |
| FR10 | Payment Reference | Vendors shall be able to submit a payment reference for organizer review. |
| FR11 | Notification | The system shall notify users about booking-related activity. |
| FR12 | Dashboard | The system shall provide dashboards for admin, organizer and vendor users. |

## Non-Functional Requirements

| ID | Requirement | Description |
| --- | --- | --- |
| NFR1 | Usability | The interface should be simple enough for non-technical users. |
| NFR2 | Accessibility | The system should be accessible through a standard web browser. |
| NFR3 | Security | Unauthorized users should not be able to access restricted functions. |
| NFR4 | Data Consistency | Stall booking status must remain consistent during booking approval. |
| NFR5 | Maintainability | The codebase should be organized using a maintainable ASP.NET MVC structure. |
| NFR6 | Responsiveness | The website should work on common desktop and mobile screen sizes. |
| NFR7 | Reliability | Core booking workflows should behave consistently during demonstration and testing. |

## Technology Stack

| Area | Selected Technology | Justification |
| --- | --- | --- |
| Backend | ASP.NET Core MVC with C# | Provides structured server-side development, authentication, authorization and database integration. |
| Frontend | HTML, CSS, JavaScript and Bootstrap | Suitable for a responsive page-oriented web application. |
| Database | SQL Server for target deployment; SQLite fallback for local development | Relational storage is suitable for users, events, stalls, bookings and payment status. |
| Authentication | ASP.NET Identity | Supports secure password handling, role management and account workflows. |
| Email | SMTP-based email sender | Supports account verification, reset links and booking-related communication. |

## Proposed System Modules

### Account Module

This module handles registration, login, logout, email verification, password reset, profile management and password change.

### Event Module

This module allows organizers to create and manage event information such as name, venue, description, category, images, dates and starting stall price.

### Stall Module

This module allows organizers to configure individual stalls with details such as stall number, name, tier, type, zone, size, price, status and map position.

### Booking Module

This module allows vendors to request stalls and organizers to review, approve or reject booking requests. It also handles payment reference updates and payment status review.

### Dashboard Module

This module provides role-specific dashboard pages for admin, organizer and vendor users.

### Notification Module

This module stores and displays booking-related notifications so users can track important system activity.

## Database Entities

The main entities proposed for the system are:

- ApplicationUser
- Event
- Stall
- Booking
- Notification
- IdentityRole

These entities support the relationship between users, events, stalls, booking requests, payment information and notifications.

## Security and Access Control

The system will use role-based access control. Organizers will be allowed to create events, manage stalls and review bookings for their own events. Vendors will be allowed to browse events, request stalls and update payment references for their own bookings. Admin users will have access to administrative dashboard information. Unauthorized access should be denied through controller-level authorization rules.

## Concurrency Control

The system will use optimistic concurrency concepts in the stall booking workflow. When an organizer approves a booking request, the system must confirm that the stall has not already been booked by another request. If the stall status has changed, the system should prevent duplicate approval and inform the organizer. This reduces the risk of double booking, which is one of the key problems identified in the Investigation Report.

## Development Methodology

The project will follow an iterative web-engineering approach. This is suitable because the system includes related interface, database, booking and security features that may require refinement during development.

The major phases are:

1. Requirement analysis.
2. System and database design.
3. Interface design and prototyping.
4. Implementation of core modules.
5. Integration of booking, role and notification workflows.
6. Functional and user-oriented testing.
7. Refinement and final documentation.

## Testing Plan

| Test Area | Purpose |
| --- | --- |
| Functional Testing | To verify that account, event, stall, booking and dashboard functions work correctly. |
| Role-Based Access Testing | To verify that users cannot access functions outside their role. |
| Concurrency Testing | To verify that duplicate stall approval is prevented. |
| Validation Testing | To verify that forms reject invalid or missing input. |
| User Acceptance Testing | To collect feedback from non-technical users on clarity, usefulness and ease of use. |

## Evaluation Plan

The system will be evaluated using technical testing and user-oriented feedback. Functional tests will confirm whether the main system modules meet the stated requirements. Access-control tests will verify correct role restrictions. Concurrency-related tests will evaluate the booking approval process. A user-acceptance questionnaire with non-technical respondents will be used to measure usability, clarity and perceived usefulness.

## Expected Deliverables

The expected deliverables are:

- Working ASP.NET MVC web application.
- Database structure for users, events, stalls, bookings and notifications.
- Role-based dashboards.
- Event and stall management features.
- Booking request and approval workflow.
- Payment reference tracking.
- Testing evidence and evaluation summary.
- Final documentation and screenshots.

## Project Schedule

| Phase | Activities |
| --- | --- |
| Weeks 1-2 | Requirement clarification, topic refinement and Investigation Report preparation. |
| Weeks 3-5 | System design, database design, use case specification and interface planning. |
| Weeks 6-10 | Implementation of event, stall, booking, organizer and vendor modules. |
| Weeks 11-12 | Concurrency-control logic, role-based access control and integration testing. |
| Weeks 13-14 | User acceptance testing, refinement and final documentation. |

## Risks and Mitigation

| Risk | Impact | Mitigation |
| --- | --- | --- |
| Double booking during approval | High | Use stall status checks and optimistic concurrency handling. |
| Incorrect role access | High | Use ASP.NET authorization attributes and role-based tests. |
| Email delivery failure | Medium | Log email failures and provide development verification links during testing. |
| Database setup issues | Medium | Use SQL Server for target deployment and SQLite fallback for local development. |
| User interface confusion | Medium | Use simple navigation, clear status labels and user acceptance feedback. |

## Ethical Considerations

The planned evaluation involves adult participants, voluntary participation and non-sensitive feedback about system usability. Participants should be informed about the purpose of the study and should be allowed to withdraw at any time. The evaluation should avoid collecting unnecessary personal data.

## Conclusion

StallBazar is a focused web-based system designed to solve practical stall booking problems for small-scale event organizers and vendors. The project addresses a real operational gap by combining event listing, stall layout, booking requests, approval workflow, payment reference tracking, role-based access control and concurrency-safe booking confirmation. The proposed system is feasible within an undergraduate final year project scope and aligns with the Investigation Report's aim, objectives and methodology.
