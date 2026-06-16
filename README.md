# JobTrackr Backend

JobTrackr is a learning-focused .NET 8 Web API project for building backend engineering skills step by step.

The goal is to grow this project into a practical backend portfolio API using C#, ASP.NET Core, SQL Server, clean architecture basics, authentication, and production-style API development.

## Current Progress

Completed so far:

- Created solution structure
- Created ASP.NET Core Web API project
- Created Application, Domain, and Infrastructure class libraries
- Added project references
- Created first domain entities
- Added task create endpoint
- Added get all tasks endpoint
- Added get task by id endpoint
- Added update task endpoint
- Added delete task endpoint
- Added complete task endpoint
- Added reopen task endpoint
- Added user create endpoint
- Added get all users endpoint
- Added get user by id endpoint
- Added update user endpoint
- Added delete user endpoint
- Connected tasks to users with `UserId`
- Added basic task title validation
- Removed default WeatherForecast API
- Added Entity Framework Core
- Added SQL Server persistence
- Added database-backed user service
- Added database-backed task service
- Added explicit task-user relationship in EF Core
- Added endpoint to get tasks for one user
- Added `userId` filtering to the task list endpoint

## Architecture

```text
JobTrackr.Api
JobTrackr.Application
JobTrackr.Domain
JobTrackr.Infrastructure
```

### JobTrackr.Api

Contains API controllers and application startup configuration.

### JobTrackr.Application

Contains DTOs and service interfaces.

### JobTrackr.Domain

Contains core business entities.

### JobTrackr.Infrastructure

Contains EF Core database context, migrations, and database-backed service implementations.

## Current Endpoints

```http
GET /api/users
POST /api/users
GET /api/users/{id}
GET /api/users/{userId}/tasks
PUT /api/users/{id}
DELETE /api/users/{id}
GET /api/tasks
GET /api/tasks?isCompleted=true
GET /api/tasks?isCompleted=false
GET /api/tasks?search=resume
GET /api/tasks?userId=1
GET /api/tasks?isCompleted=true&search=resume
GET /api/tasks?userId=1&isCompleted=false&search=resume
POST /api/tasks
GET /api/tasks/{id}
PUT /api/tasks/{id}
DELETE /api/tasks/{id}
PATCH /api/tasks/{id}/complete
PATCH /api/tasks/{id}/reopen
```

Current behavior:

- `GET /api/users` returns all users from SQL Server.
- `POST /api/users` creates a user in SQL Server.
- `GET /api/users/{id}` returns one user or `404 Not Found`.
- `GET /api/users/{userId}/tasks` returns tasks for one user, returns an empty list if the user has no tasks, and returns `404 Not Found` if the user does not exist.
- `PUT /api/users/{id}` updates user full name and email.
- `DELETE /api/users/{id}` deletes a user or returns `404 Not Found`.
- `POST /api/tasks` requires `UserId`, creates a task for an existing user, and returns `201 Created`.
- `GET /api/tasks` returns all tasks and can optionally filter by completion status.
- `GET /api/tasks?isCompleted=true` returns completed tasks.
- `GET /api/tasks?isCompleted=false` returns incomplete tasks.
- `GET /api/tasks?search=resume` searches tasks by title.
- `GET /api/tasks?userId=1` returns tasks for one user.
- `GET /api/tasks?isCompleted=true&search=resume` combines completion filtering and title search.
- `GET /api/tasks?userId=1&isCompleted=false&search=resume` combines user, completion, and title filters.
- `GET /api/tasks/{id}` returns one task or `404 Not Found`.
- `PUT /api/tasks/{id}` updates task title and description.
- `DELETE /api/tasks/{id}` deletes a task or returns `404 Not Found`.
- `PATCH /api/tasks/{id}/complete` marks a task as completed.
- `PATCH /api/tasks/{id}/reopen` marks a completed task as not completed.
- Creating a task with a missing user returns `400 Bad Request`.
- Empty task title returns `400 Bad Request`.

## Database

JobTrackr uses SQL Server with Entity Framework Core.

Current database features:

- `AppDbContext`
- EF Core migrations
- `Users` table
- `Tasks` table
- foreign key relationship from `Tasks.UserId` to `Users.Id`
- cascade delete from user to that user's tasks

## Run The Project

From the project root:

```bash
dotnet build
dotnet run --project src/JobTrackr.Api
```

Then open Swagger:

```text
https://localhost:PORT/swagger
```

## Learning Goal

This project is part of a long-term backend engineering journey focused on:

- C#
- ASP.NET Core Web API
- SQL Server
- backend architecture
- DSA/problem solving
- interview communication

## Next Steps

Planned future work:

- better task query options
- due dates and task priority
- stronger validation
- better error handling
- authentication
- unit tests
- deployment basics
