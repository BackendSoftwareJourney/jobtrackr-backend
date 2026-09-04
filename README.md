# JobTrackr Backend

JobTrackr is a learning-focused .NET 8 Web API project built step by step using production-style backend practices.

The project currently includes users, user-owned tasks, SQL Server persistence, JWT authentication, structured error handling, request logging, health checks, automated service tests, and GitHub Actions CI.

## The First 90 Days Book

The first 90 days of the JobTrackr backend journey are available as a 330-page PDF book:

[Read or download Backend Software Journey: The First 90 Days](docs/book/backend-software-journey-90-days.pdf)

The book follows the project from initial solution setup through SQL Server persistence, authentication, task ownership, testing, continuous integration, reliability improvements, and the Month 3 review.

## Current Features

- ASP.NET Core Web API targeting .NET 8
- Api, Application, Domain, and Infrastructure projects
- SQL Server persistence with Entity Framework Core
- EF Core migrations
- User CRUD endpoints
- Task CRUD endpoints
- Task completion and reopen endpoints
- Optional task due dates
- Low, Medium, and High task priority values
- Task filtering by completion status and title search
- Task pagination with page metadata
- Task sorting by creation date and due date
- Password hashing
- User registration and login
- Authenticated password changes
- JWT token generation
- JWT-protected task endpoints
- Authenticated user profile retrieval and update
- User-owned task creation, listing, retrieval, update, and deletion
- Global exception handling with safe `ProblemDetails` responses
- Structured `ProblemDetails` responses for known API errors
- Request DTO validation with field-level errors
- Basic request logging for method, path, status code, and elapsed time
- Public `GET /health` application health endpoint
- XML endpoint descriptions in Swagger
- Swagger JWT Bearer authorization support
- Environment-specific local configuration
- Optional development-only seed data
- JWT signing key stored outside Git with .NET User Secrets
- Documented local SQL Server, migration, HTTPS, and authentication setup
- xUnit service and API integration tests using EF Core InMemory
- GitHub Actions Release build and automated test execution

## Architecture

```text
JobTrackr.Api
JobTrackr.Application
JobTrackr.Domain
JobTrackr.Infrastructure
JobTrackr.Tests
```

### JobTrackr.Api

Contains API controllers, middleware, authentication configuration, dependency registration, and application startup.

### JobTrackr.Application

Contains request and response DTOs, service interfaces, shared messages, password hashing, and authentication contracts.

### JobTrackr.Domain

Contains the core `User` and `JobTask` entities.

### JobTrackr.Infrastructure

Contains EF Core database access, migrations, SQL Server service implementations, authentication service logic, and JWT token generation.

### JobTrackr.Tests

Contains xUnit service tests and HTTP integration tests using isolated EF Core in-memory databases.

## Current Endpoints

### Health

```http
GET /health
```

The health endpoint is public and returns `200 OK` with `Healthy` while the API is running.

It is mapped directly in `Program.cs`, so it may not appear in Swagger.

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
PUT /api/auth/change-password
```

Registration and login are public. Changing a password requires a valid Bearer token for the authenticated user.

### Tasks

Task endpoints require a valid Bearer token.

```http
GET /api/tasks
GET /api/tasks?isCompleted=true
GET /api/tasks?search=resume
GET /api/tasks?sortBy=createdAt&sortDirection=desc
GET /api/tasks?sortBy=dueDate&sortDirection=asc
GET /api/tasks?pageNumber=1&pageSize=10
GET /api/tasks?isCompleted=false&search=resume&sortBy=dueDate&sortDirection=asc&pageNumber=1&pageSize=10
POST /api/tasks
GET /api/tasks/{id}
PUT /api/tasks/{id}
DELETE /api/tasks/{id}
PATCH /api/tasks/{id}/complete
PATCH /api/tasks/{id}/reopen
```

### Users

```http
GET /api/users/me
PUT /api/users/me
GET /api/users
POST /api/users
GET /api/users/{id}
GET /api/users/{userId}/tasks
PUT /api/users/{id}
DELETE /api/users/{id}
```

The `/api/users/me` endpoints require a valid Bearer token and use the authenticated user id from JWT claims. The remaining user-id-based CRUD routes are legacy endpoints and are not yet authorization-protected.

## Authentication Behavior

- Registration rejects an email that is already registered.
- Passwords are hashed before they are stored.
- Authentication responses do not expose passwords or password hashes.
- Registration currently returns an empty token.
- Successful login returns a JWT token.
- Invalid login credentials return a safe authentication error.
- Protected endpoints require a valid `Authorization: Bearer TOKEN` header.
- An authenticated user can change their password after providing the correct current password.
- Password confirmation must match, and the new password must differ from the current password.
- Existing JWTs remain valid until they expire after a password change; token revocation is not implemented yet.

## Task Ownership Behavior

- Task creation does not accept `UserId` in the request body.
- A new task automatically belongs to the authenticated user.
- `GET /api/tasks` returns only tasks owned by the authenticated user.
- Completion and title filters apply only to the authenticated user's tasks.
- Get By Id returns a task only when the authenticated user owns it.
- Update changes a task only when the authenticated user owns it.
- Delete removes a task only when the authenticated user owns it.
- Complete changes the completion state only when the authenticated user owns the task.
- Reopen changes the completion state only when the authenticated user owns the task.
- Missing tasks and tasks owned by another user return `404 Not Found` for ownership-protected operations.

## Current Authorization Limitations

- Legacy user CRUD endpoints are not yet protected by authorization.
- `GET /api/users/{userId}/tasks` accepts a user id from the route and is not yet ownership-protected.
- Existing JWTs are not revoked after a password change and remain valid until expiration.

These limitations are planned future improvements and should not be treated as completed security behavior.

## API Reliability Behavior

- Invalid DTOs return `400 Bad Request` with field-level validation errors.
- Known bad requests and missing resources return structured `ProblemDetails` responses.
- Unexpected exceptions return a safe `500 Internal Server Error` response without exposing stack traces.
- Request logs record the HTTP method, path, status code, and elapsed time.
- Request bodies, passwords, JWT tokens, authorization headers, and signing keys are not logged.
- `GET /health` provides a basic public application health check.

## Database

JobTrackr uses SQL Server with Entity Framework Core.

Current database features:

- `AppDbContext`
- EF Core migrations
- `Users` table
- `Tasks` table
- `PasswordHash` column on users
- nullable `DueDateUtc` column on tasks
- `Priority` column on tasks
- foreign key from `Tasks.UserId` to `Users.Id`
- cascade delete from a user to that user's tasks

## Configuration

- Shared non-secret settings are stored in `appsettings.json`.
- The local SQL Server connection string is stored in `appsettings.Development.json`.
- The local JWT signing key is stored outside the repository with .NET User Secrets.
- HTTPS is the default local launch profile.
- Production secrets must be supplied through environment variables or a secure secret manager in a future deployment.

## Automated Tests

JobTrackr currently has 36 xUnit tests: 34 service tests and two API integration tests.

Current test coverage includes:

- password hashing and verification
- registration and login behavior
- valid and invalid password changes
- task creation, retrieval, update, and deletion
- task ownership authorization
- task completion and reopening
- task pagination and page metadata
- created-date and due-date sorting
- combined ownership, filtering, search, sorting, and pagination
- user creation and retrieval
- public health endpoint behavior through the ASP.NET Core HTTP pipeline
- registration and login through HTTP, including matching user identity and a non-empty login token

Task and user service tests use `Microsoft.EntityFrameworkCore.InMemory`, so they do not connect to SQL Server.

The health and authentication integration tests use `WebApplicationFactory<Program>` to start the API in memory under the Testing environment. They use test-only configuration and do not require a running API, SQL Server, or local User Secrets. The authentication test verifies a token is returned; using that token against protected task endpoints is planned next.

The current API has also passed a manual regression flow covering health, registration, login, validation, task CRUD and filters, two-user ownership isolation, user CRUD, structured errors, and request logging.

### Run all tests

From the project root:

```powershell
dotnet restore JobTrackr.slnx
dotnet build JobTrackr.slnx --configuration Release --no-restore
dotnet test JobTrackr.slnx --configuration Release --no-build --no-restore
```

Expected result:

```text
Passed: 36
Failed: 0
Skipped: 0
```

## GitHub Actions

The workflow is stored at:

```text
.github/workflows/build.yml
```

On pushes and pull requests to `main`, GitHub Actions:

1. Checks out the repository.
2. Installs the .NET 8 SDK.
3. Restores API dependencies.
4. Restores test dependencies.
5. Builds the API and production projects in Release configuration.
6. Runs all automated tests.

The tests use an in-memory database, so the workflow does not require SQL Server or repository secrets.

## Local Environment Setup

### Prerequisites

Install or provide:

- .NET 8 SDK
- SQL Server
- SQL Server Management Studio or another SQL client
- EF Core command-line tools 8.x
- Git

Confirm the .NET SDK:

```powershell
dotnet --version
```

The installed SDK must support projects targeting `net8.0`.

Confirm the EF Core tools:

```powershell
dotnet ef --version
```

If `dotnet ef` is not installed, install the .NET 8 version:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.27
```

### Restore Dependencies

From the repository root:

```powershell
cd C:\path\to\JobTrackr
dotnet restore JobTrackr.slnx
```

Replace `C:\path\to\JobTrackr` with the actual repository location.

### Configure SQL Server

The Development environment reads its local connection string from:

```text
src\JobTrackr.Api\appsettings.Development.json
```

The default local configuration is:

```text
Server=localhost;Database=JobTrackrDb;Trusted_Connection=True;TrustServerCertificate=True
```

Requirements:

- SQL Server must be running locally.
- Windows authentication must be available for the current Windows user.
- The current user must be allowed to create or update `JobTrackrDb`.

If the SQL Server instance name differs, update only the appropriate local development configuration. Never commit a database password.

Apply the existing EF Core migrations:

```powershell
dotnet ef database update --project src\JobTrackr.Infrastructure --startup-project src\JobTrackr.Api
```

This creates or updates `JobTrackrDb` using the migrations stored in the Infrastructure project.

### Configure The JWT Signing Key

The repository does not contain a JWT signing key.

Generate and store a local key with .NET User Secrets:

```powershell
$jwtKey = [Guid]::NewGuid().ToString("N") + [Guid]::NewGuid().ToString("N")
dotnet user-secrets set "Jwt:Key" "$jwtKey" --project src\JobTrackr.Api
Remove-Variable jwtKey
```

The `UserSecretsId` is tracked in the API project file, but the secret value is stored outside the repository in the current Windows user profile.

Do not add the JWT key to `appsettings.json`, `appsettings.Development.json`, source code, documentation, or Git.

### Optional Development Seed Data

Development seeding is disabled by default. Apply the existing EF Core migrations first, then enable seeding locally without changing tracked configuration:

```powershell
dotnet user-secrets set "SeedData:Enabled" "true" --project src\JobTrackr.Api
dotnet run --project src\JobTrackr.Api --launch-profile https
```

When the API starts in Development, it creates one sample user and three sample tasks if the seed user does not already exist:

```text
Email: developer@jobtrackr.local
Password: Development123!
```

These credentials are for local development only. Do not reuse them for a real account or production environment. The password is hashed before it is stored in SQL Server.

The seeder checks the dedicated email before inserting data, so restarting the API does not create duplicate sample rows. It runs only when the environment is Development and `SeedData:Enabled` is true. It does not apply migrations or reset the database.

Disable the local override with:

```powershell
dotnet user-secrets remove "SeedData:Enabled" --project src\JobTrackr.Api
```

Removing the setting prevents future seed runs but does not delete rows already added to the local database.

### Reset The Local Development Database

> **Warning:** This permanently deletes every user and task in the local `JobTrackrDb` database. Use it only for disposable local development data. Never run these commands against a shared or production database.

Stop the running API before resetting the database. Back up any local data you need to keep.

From the repository root, disable optional seed data:

```powershell
cd C:\path\to\JobTrackr
dotnet user-secrets remove "SeedData:Enabled" --project .\src\JobTrackr.Api\JobTrackr.Api.csproj
```

Inspect and validate the tracked Development connection string:

```powershell
$settings = Get-Content .\src\JobTrackr.Api\appsettings.Development.json | ConvertFrom-Json
$connectionString = $settings.ConnectionStrings.DefaultConnection
$connectionString

$isLocalServer = $connectionString -match '(?i)(?:^|;)\s*(?:Server|Data Source)\s*=\s*localhost\s*(?:;|$)'
$isJobTrackrDatabase = $connectionString -match '(?i)(?:^|;)\s*(?:Database|Initial Catalog)\s*=\s*JobTrackrDb\s*(?:;|$)'

if (!$isLocalServer -or !$isJobTrackrDatabase)
{
    throw "Reset stopped: expected the local JobTrackrDb connection string."
}
```

The displayed connection string must contain `Server=localhost` and `Database=JobTrackrDb`.

Preview the target using EF Core's interactive confirmation:

```powershell
dotnet ef database drop --project .\src\JobTrackr.Infrastructure\JobTrackr.Infrastructure.csproj --startup-project .\src\JobTrackr.Api\JobTrackr.Api.csproj
```

EF Core displays the database and server it is about to delete. Confirm that the prompt names `JobTrackrDb` on `localhost`, then type `N` and press Enter to cancel this first check safely.

When you intentionally want to reset the confirmed local database, run the same command again:

```powershell
dotnet ef database drop --project .\src\JobTrackr.Infrastructure\JobTrackr.Infrastructure.csproj --startup-project .\src\JobTrackr.Api\JobTrackr.Api.csproj
```

Type `Y` only when the prompt identifies `JobTrackrDb` on `localhost`. Otherwise, type `N`.

Recreate the empty database by applying all migrations:

```powershell
dotnet ef database update --project .\src\JobTrackr.Infrastructure\JobTrackr.Infrastructure.csproj --startup-project .\src\JobTrackr.Api\JobTrackr.Api.csproj
```

Refresh SQL Server Management Studio and confirm `JobTrackrDb` contains:

```text
dbo.Users
dbo.Tasks
dbo.__EFMigrationsHistory
```

The `Users` and `Tasks` tables should be empty. The migration-history table should contain all five migrations. Development seeding remains disabled unless it is explicitly enabled again through User Secrets.

### Trust The Development HTTPS Certificate

Run once on the development machine:

```powershell
dotnet dev-certs https --trust
```

Accept the Windows trust prompt if it appears.

### Build And Test

```powershell
dotnet build JobTrackr.slnx --configuration Release
dotnet test JobTrackr.slnx --configuration Release --no-build
```

Expected test result:

```text
Passed: 36
Failed: 0
Skipped: 0
```

The automated tests use EF Core InMemory and do not connect to SQL Server.

### Run The API

```powershell
dotnet run --project src\JobTrackr.Api --launch-profile https
```

Default local addresses:

```text
https://localhost:7024
http://localhost:5123
```

Open Swagger:

```text
https://localhost:7024/swagger
```

Check API health:

```text
https://localhost:7024/health
```

Expected health response:

```text
Healthy
```

### Test Authentication

1. Call `POST /api/auth/register` to create a user if needed.
2. Call `POST /api/auth/login` with the registered email and password.
3. Copy only the JWT value returned in the `token` property.
4. Select **Authorize** at the top of Swagger UI.
5. Paste only the JWT token; Swagger adds the `Bearer` prefix automatically.
6. Select **Authorize** and close the dialog.
7. Call `GET /api/tasks` or another protected endpoint.

With a valid token, the request succeeds for the authenticated user. After using **Logout** in the Authorize dialog, protected requests return `401 Unauthorized`.

PowerShell can also be used to test a protected endpoint:

```powershell
$token = "PASTE_LOGIN_TOKEN_HERE"
Invoke-RestMethod `
    -Uri "https://localhost:7024/api/tasks" `
    -Headers @{ Authorization = "Bearer $token" }
Remove-Variable token
```

Without a valid token, protected task endpoints return:

```text
401 Unauthorized
```

With a valid Bearer token, the authenticated user can access their task endpoints.

### Local Configuration Safety

Never commit:

- JWT signing keys
- database passwords
- access tokens
- user passwords
- production connection strings

For local development, use User Secrets for secret values. A future deployment must provide secrets through environment variables or a secure secret manager.

## Learning Goal

This project is part of a long-term backend engineering journey focused on:

- C#
- ASP.NET Core Web API
- SQL Server
- backend architecture
- authentication and authorization
- automated testing
- continuous integration
- deployment
- interview communication

## Next Steps

Planned work includes:

- stronger authorization for user endpoints
- broader controller and integration test coverage
- database-aware health checks when operationally useful
- deployment fundamentals
