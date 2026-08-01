# Technology stack

## Backend and runtime

| Area | Technology | Repository evidence | Role |
| --- | --- | --- | --- |
| Runtime | .NET 10 | `TargetFramework` is `net10.0` | Hosts the web application |
| Language | C# with nullable reference types and implicit usings | `StallBazar.csproj` | Application and domain logic |
| Web framework | ASP.NET Core MVC | `AddControllersWithViews`, controllers and Razor views | Routing, controllers, model binding, validation, HTML rendering |
| Authentication | ASP.NET Core Identity EF Core 10.0.9 | Package reference and `AddIdentity` | Users, passwords, email tokens, cookies and roles |
| ORM | Entity Framework Core 10.0.9 | DbContext and provider packages | Relational mapping, queries, transactions, updates |
| Email | `System.Net.Mail.SmtpClient` behind `IEmailSender` | `ConsoleEmailSender` | Verification, password reset and approval email |
| Logging | Microsoft console logging in development | `Program.cs` | Application and email-delivery diagnostics |

## Persistence

| Environment | Provider | Selection rule |
| --- | --- | --- |
| Production/non-development | SQL Server via EF Core 10.0.9 | Always uses `DefaultConnection` |
| Development with reachable SQL Server | SQL Server via EF Core 10.0.9 | A startup connection probe succeeds |
| Development without reachable SQL Server | SQLite via EF Core 10.0.9 | Automatic fallback to `stallbazar-dev.db` |

Additional database dependency: `SQLitePCLRaw.bundle_e_sqlite3` 3.0.3 supplies the native SQLite bundle.

Database initialization currently uses:

- `Database.EnsureCreatedAsync()`.
- Seeded Admin, Organizer, and Vendor roles.
- Seeded demonstration users and a sample event/stall layout.
- SQL Server-only `COL_LENGTH`/`OBJECT_ID` guards for selected later columns/table additions.

## Frontend

| Area | Technology | Usage |
| --- | --- | --- |
| Rendering | Razor `.cshtml` views | Server-rendered pages and role-aware UI |
| Markup | HTML5 | Semantic page/form structure |
| Styling | Bootstrap plus custom CSS | Responsive layout, components, branded experience |
| Scripting | Vanilla JavaScript and jQuery assets | Page interactions and validation support |
| Validation | jQuery Validation and Unobtrusive Validation | Client-side MVC form feedback, backed by server validation |
| Images | SVG/remote imagery/local uploads | Branding, event photography, maps and profiles |

Frontend libraries are vendored under `wwwroot/lib`; no Node-based frontend build pipeline is required to run the MVC application. `qa/browser-check.mjs` is a separate browser-check script, not an application runtime dependency.

## Storage and file handling

- Event photos: `wwwroot/uploads/events`.
- Event map images: `wwwroot/uploads/maps`.
- Profile images: `wwwroot/uploads/profiles`.
- Development data-protection keys: `.keys` under the content root.
- Development SQLite database: `stallbazar-dev.db` under the content root.

Local file storage assumes a persistent writable application directory. Container or multi-instance deployment should replace it with shared/object storage and a shared data-protection key ring.

## Framework configuration

### Identity policy

- Unique email required.
- Confirmed email required before sign-in.
- Minimum password length 8.
- Digit, lowercase, and uppercase required.
- Non-alphanumeric character not required.
- Registration/profile validation additionally restricts addresses to `@gmail.com`.

### HTTP pipeline

1. Production exception handler and HSTS.
2. Production HTTPS redirection.
3. Routing.
4. Authentication.
5. Authorization.
6. Static assets.
7. Conventional controller route: `{controller=Home}/{action=Index}/{id?}`.

## Configuration inventory

| Key | Purpose | Recommended source |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection | Environment/deployment secret store |
| `Smtp:Host` | SMTP host | Environment-specific configuration |
| `Smtp:Port` | SMTP TLS port | Environment-specific configuration |
| `Smtp:Username` | SMTP login/from fallback | Secret store |
| `Smtp:Password` | SMTP credential | Secret store only |
| `Smtp:From` | Sender address | Environment-specific configuration |
| `SupportEmail` | Contact-form recipient | Environment-specific configuration |
| `Logging:LogLevel` | Logging thresholds | `appsettings` or environment override |

Do not store live credentials in committed configuration. Rotate any credential that has already been committed and remove it from repository history where appropriate.

## Development commands

Typical commands from the project root:

```powershell
dotnet restore
dotnet build
dotnet run
```

The launch profile in `Properties/launchSettings.json` defines local URLs/environment settings. If LocalDB is unavailable in Development, the application should start with its SQLite fallback.

## Technology constraints and upgrade notes

- Keep all Microsoft ASP.NET Core/EF Core packages aligned with the target framework and each other.
- Test both SQL Server and SQLite because provider-specific behavior can differ, especially conditional updates and row-version handling.
- `SmtpClient` is adequate for the current small synchronous workflow, but a queued email provider is preferable for production reliability.
- Introduce EF Core migrations before schema evolution becomes frequent or deployment data must be preserved reliably.
