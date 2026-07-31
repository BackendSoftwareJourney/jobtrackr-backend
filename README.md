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

## Run The API

From the project root:

```powershell
dotnet restore src\JobTrackr.Api\JobTrackr.Api.csproj
dotnet build src\JobTrackr.Api\JobTrackr.Api.csproj
dotnet run --project src\JobTrackr.Api
```

Open the Swagger URL shown in the terminal:

```text
https://localhost:PORT/swagger
```

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
