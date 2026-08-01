# StallBazar project documentation

This folder documents the current StallBazar implementation. It is based on the source code in this repository, with the project specification used for context. Where the specification and implementation differ, the implementation is treated as the current source of truth.

## Document index

| Document | Purpose |
| --- | --- |
| [01 - Project overview](./01-project-overview.md) | Purpose, actors, scope, modules, and implemented capabilities |
| [02 - System architecture](./02-system-architecture.md) | Runtime topology, application layers, storage, and key design decisions |
| [03 - Process flows](./03-process-flows.md) | Registration, event setup, booking, payment, approval, and concurrency flows |
| [04 - Data model and ERD](./04-data-model-erd.md) | Core entities, keys, relationships, statuses, and persistence rules |
| [05 - Wireframes](./05-wireframes.md) | Low-fidelity layouts for the public pages and role-based workspaces |
| [06 - Technology stack](./06-technology-stack.md) | Frameworks, packages, frontend assets, database providers, and configuration |
| [07 - Access control and routes](./07-access-control-and-routes.md) | Role/ownership matrix and controller action inventory |
| [08 - Requirements traceability](./08-requirements-traceability.md) | Mapping from stated requirements to the implemented code |

## System at a glance

StallBazar is a server-rendered ASP.NET Core MVC application for publishing events, configuring stalls, receiving vendor booking requests, recording deposit references, and approving or rejecting requests without double-booking a stall.

The application has four access contexts:

- Public visitors browse events and view stall availability.
- Vendors request stalls, submit payment references, cancel pending requests, and track outcomes.
- Organizers manage their own events and stalls and review requests for those events.
- Administrators see platform-level counts and recent activity.

## Diagram format

Architecture, process, and data diagrams use Mermaid inside Markdown. GitHub, GitLab, many IDE Markdown previews, and Mermaid-enabled documentation tools can render them directly.

## Source-of-truth notes

- Runtime and dependency facts come from `Program.cs` and `StallBazar.csproj`.
- Data relationships come from `Data/ApplicationDbContext.cs` and the entity models.
- Process and authorization facts come from the controllers.
- Wireframes reflect the current Razor view hierarchy and actions, not a proposed redesign.
- Generated folders, local databases, logs, uploaded media, and investigation-report artifacts are not treated as application design sources.

## Maintenance rule

Update these documents when a controller route, role rule, entity relationship, database provider, or major page structure changes. The most likely files to require synchronized edits are the architecture, process-flow, ERD, access-control, and traceability documents.
