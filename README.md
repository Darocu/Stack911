# Stack911

Stack911 is a .NET solution that integrates with the CentralSquare API, providing access to its endpoints through a RESTful interface. It also includes background services to support various functionalities related to 911 public safety operations.

The solution targets .NET Framework 4.8.1 (net481).

## Projects in this solution

- src/ApiService
  - Type: ASP.NET Web API/MVC application
  - Purpose: Primary HTTP API surface for Stack911. Exposes controllers for Incidents, Radios, Units, Personnel, Positions, and supporting middleware such as logging and error handling.
  - Notable folders:
    - Controllers: API endpoints (e.g., IncidentsController, RadiosController)
    - EventHandlers: Command/event handling for various domains (Incidents, Units, Personnel, Radios, Positions)
    - Middleware: LoggingHandler, ErrorHandler, ApiKey authorization utilities
    - Services: Supporting services (e.g., ApiKeyStore)

- src/IncidentEventService
  - Type: .NET Framework background service/worker
  - Purpose: Processes and reacts to incident-related events.
  - Notable folders: EventHandlers, Models

- src/UnitEventService
  - Type: .NET Framework background service/worker
  - Purpose: Processes and reacts to unit-related events.
  - Notable folders: EventHandlers, Models

## Requirements

- .NET Framework 4.8.1 (net481)
- Visual Studio or JetBrains Rider (recommended) on Windows

## Getting started

1. Open Stack911.sln in Rider or Visual Studio.
2. Restore NuGet packages if prompted.
3. Set the desired startup project:
   - ApiService for running the HTTP API (recommended for development/testing)
   - IncidentEventService or UnitEventService for running background workers
4. Build and run.

## Configuration

- See the `config` folder (e.g., TODO.md) and the `src/ApiService` project for configuration, logging, and authorization setup.
- The ApiService uses middleware components for logging and error handling, and an API key store for authorization.

## Repository layout

- Stack911.sln — Solution file
- README.md — This file
- config/ — Additional configuration and notes
- logs/ — Default log output location(s)
- src/ — All source projects
  - ApiService/ — Main API
  - IncidentEventService/ — Incident events worker
  - UnitEventService/ — Unit events worker
