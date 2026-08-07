# JobTrackr Backend

JobTrackr is a learning-focused .NET 8 Web API project built step by step using production-style backend practices.

The project currently includes users, user-owned tasks, SQL Server persistence, JWT authentication, automated service tests, and GitHub Actions CI.

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
- Password hashing
- User registration and login
- JWT token generation
- JWT-protected task endpoints
- User-owned task creation, listing, retrieval, update, and deletion
- Global exception handling
- Request DTO validation
- xUnit service tests using EF Core InMemory
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

Contains xUnit service tests using an isolated EF Core in-memory database.

## Current Endpoints

### Authentication

```http
POST /api/auth/register
POST /api/auth/login
```

### Tasks

Task endpoints require a valid Bearer token.

```http
GET /api/tasks
GET /api/tasks?isCompleted=true
GET /api/tasks?isCompleted=false
GET /api/tasks?search=resume
GET /api/tasks?isCompleted=false&search=resume
POST /api/tasks
GET /api/tasks/{id}
PUT /api/tasks/{id}
DELETE /api/tasks/{id}
PATCH /api/tasks/{id}/complete
PATCH /api/tasks/{id}/reopen
```

### Users

```http
GET /api/users
POST /api/users
GET /api/users/{id}
GET /api/users/{userId}/tasks
PUT /api/users/{id}
DELETE /api/users/{id}
```

## Authentication Behavior

- Registration rejects an email that is already registered.
- Passwords are hashed before they are stored.
- Authentication responses do not expose passwords or password hashes.
- Registration currently returns an empty token.
- Successful login returns a JWT token.
- Invalid login credentials return a safe authentication error.
- Protected task endpoints require `Authorization: Bearer TOKEN`.

## Task Ownership Behavior

- Task creation does not accept `UserId` in the request body.
- A new task automatically belongs to the authenticated user.
- `GET /api/tasks` returns only tasks owned by the authenticated user.
- Completion and title filters apply only to the authenticated user's tasks.
- Get By Id returns a task only when the authenticated user owns it.
- Update changes a task only when the authenticated user owns it.
- Delete removes a task only when the authenticated user owns it.
- Missing tasks and tasks owned by another user return `404 Not Found` for ownership-protected operations.

## Current Authorization Limitations

- Complete and Reopen require authentication but do not yet check task ownership.
- User CRUD endpoints are not yet protected by authorization.

These limitations are planned future improvements and should not be treated as completed security behavior.

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

## Automated Tests

JobTrackr currently has 15 xUnit service tests.

Current test coverage includes:

- password hashing and verification
- registration
- valid login
- invalid-password login
- valid task creation
- empty task title validation
- existing and missing task retrieval
- valid task update
- missing-task update
- empty-title update validation
- successful and missing task deletion
- valid user creation
- existing user retrieval

Task and user service tests use `Microsoft.EntityFrameworkCore.InMemory`, so they do not connect to SQL Server.

### Run all tests

From the project root:

```powershell
dotnet restore tests\JobTrackr.Tests\JobTrackr.Tests.csproj
dotnet test tests\JobTrackr.Tests\JobTrackr.Tests.csproj --configuration Release --no-restore
```

Expected result:

```text
Passed: 15
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
dotnet restore src\JobTrackr.Api\JobTrackr.Api.csproj
dotnet restore tests\JobTrackr.Tests\JobTrackr.Tests.csproj
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

### Trust The Development HTTPS Certificate

Run once on the development machine:

```powershell
dotnet dev-certs https --trust
```

Accept the Windows trust prompt if it appears.

### Build And Test

```powershell
dotnet build src\JobTrackr.Api\JobTrackr.Api.csproj --configuration Release
dotnet test tests\JobTrackr.Tests\JobTrackr.Tests.csproj --configuration Release
```

Expected test result:

```text
Passed: 15
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

1. Call `POST /api/auth/register` to create a user.
2. Call `POST /api/auth/login` with the registered email and password.
3. Copy the JWT token returned by login.

Swagger can be used for registration and login. The current Swagger configuration does not yet provide a Bearer-token **Authorize** button.

Test the protected task endpoint from PowerShell:

```powershell
$token = "PASTE_LOGIN_TOKEN_HERE"
Invoke-RestMethod -Uri "https://localhost:7024/api/tasks" -Headers @{ Authorization = "Bearer $token" }
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

- owner protection for Complete and Reopen
- stronger authorization for user endpoints
- broader automated test coverage
- request logging
- API reliability improvements
- deployment fundamentals
