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
- Added basic task title validation
- Removed default WeatherForecast API

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

Contains DTOs, service interfaces, and application use-case logic.

### JobTrackr.Domain

Contains core business entities.

### JobTrackr.Infrastructure

Reserved for database and external system code later.

## Current Endpoints

```http
GET /api/tasks
POST /api/tasks
GET /api/tasks/{id}
PUT /api/tasks/{id}
DELETE /api/tasks/{id}
```

Current behavior:

- `POST /api/tasks` creates a task and returns `201 Created`.
- `GET /api/tasks` returns all tasks.
- `GET /api/tasks/{id}` returns one task or `404 Not Found`.
- `PUT /api/tasks/{id}` updates task title and description.
- `DELETE /api/tasks/{id}` deletes a task or returns `404 Not Found`.
- Empty task title returns `400 Bad Request`.

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

- SQL Server persistence
- Entity Framework Core
- authentication
- unit tests
- deployment basics
