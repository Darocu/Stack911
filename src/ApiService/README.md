# ApiService

ApiService is a RESTful HTTP API that exposes selected capabilities of the Cincinnati Unified Safety Toolkit (CUST) / TriTech VisiCAD to internal clients.
It provides secure, key-based access to incidents, units, positions, and personnel radios, and enforces fine‑grained permissions per endpoint.

## What it does
- Serves versioned REST endpoints under `/api/v1/`:
  - Incidents: list active incidents, fetch by ID, create incidents, add comments, and update incident fields
  - Units: list active units and fetch a unit by name
  - Positions: refresh positions from the CAD data source, update the cache, and return current positions
  - Radios: manage personnel radio assignments (create, update, delete) and query personnel/radio relationships
- Authenticates requests via API Key header and translates keys to claims/permissions
- Authorizes access per action using permission requirements (e.g., IncidentView, UnitView, Administrator)
- Provides interactive Swagger documentation and test UI at `/swagger`
- Centralizes error handling and logs operational events/exceptions to rolling log files

## Endpoint overview
Base path for v1: `/api/v1/`

- Incidents (`/incidents`)
  - GET `/api/v1/incidents` — list active incidents (RequirePermission: IncidentView)
  - GET `/api/v1/incidents/{incidentId}` — get incident by numeric ID or CAD number (IncidentView)
  - GET `/api/v1/incidents/userdefinedfields/{agencyName}` — list UDFs for an agency (IncidentView)
  - GET `/api/v1/incidents/callmethods/{agencyName}` — list call methods for an agency (IncidentView)
  - GET `/api/v1/incidents/callertypes/{agencyName}` — list caller types for an agency (IncidentView)
  - POST `/api/v1/incidents/create` — create an incident (IncidentEdit)
  - POST `/api/v1/incidents/{incidentId}/comment` — add a comment (IncidentEdit)
  - POST `/api/v1/incidents/{incidentId}/calltakenby/{employeeId}` — set "call taken by" (IncidentEdit)
  - POST `/api/v1/incidents/{incidentId}/callmethod/{callMethod}` — update call method (IncidentEdit)
  - POST `/api/v1/incidents/{incidentId}/callertype/{callerType}` — update caller type (IncidentEdit)
  - POST `/api/v1/incidents/{incidentId}/userdefinedfield` — update a user-defined field (IncidentEdit)

- Units (`/units`)
  - GET `/api/v1/units` — list active units (UnitView)
  - GET `/api/v1/units/{unitName}` — get details for a unit (UnitView)

- Positions (`/positions`)
  - GET `/api/v1/positions` — refresh and return tracked positions (PositionView)

- Radios (`/radios`)
  - GET `/api/v1/radios` — list personnel and their radios for a configured scope (RadioView)
  - POST `/api/v1/radios/create` — create/add a radio assignment (Administrator)
  - POST `/api/v1/radios/{id}/update` — change a radio assignment (Administrator)
  - DELETE `/api/v1/radios/{id}/delete` — remove a radio assignment (Administrator)

Note: Actual request/response shapes are defined by the controller models in `src/ApiService/Models/*` and the backing event handlers in `src/ApiService/EventHandlers/*`.

## Authentication and authorization
- Authentication: Provide an API key in the `X-API-Key` header.
  - Example: `X-API-Key: 1e3aec2f-c281-4bc8-bbca-7d04c0c5089e`
- Authorization: Each endpoint uses `[RequirePermission("...")]` to enforce access.
  - Keys are mapped to permission claims via configuration (see `ApiKeys` in appsettings).
  - Possession of the `Administrator` permission grants access to all endpoints.
- The home page (`/`) and Swagger (`/swagger`) are accessible without an API key for discovery only; API calls still require a key.

## Configuration
Configuration is loaded at runtime by `StartupConfig` and expected at one of the following paths:
- Development: `../config/apiservice_appsettings.json` relative to the executable
- Deployed: `./config/apiservice_appsettings.json` relative to the executable

Important settings (see `config/ApiService_appsettings.json`):
- `ServiceAccountName` and `ServiceDisplayName`
- `ECCDATAConnectionString` — connection string for the CAD data source
- `ApiKeys` — list of API keys with their permissions and descriptions

## Logging and error handling
- Serilog is configured by `StartupConfig.ConfigureSerilog` to write daily rolling logs to the `logs` folder.
- `Middleware/ErrorHandler` catches unhandled exceptions and returns `500 Internal Server Error` while logging details.

## Swagger and exploration
- Swagger is enabled via `App_Start/SwaggerConfig.cs`.
- Visit `/swagger` for interactive documentation and to try endpoints.
- In Swagger UI, use the API key via the "Authorize" button or ensure the `X-API-Key` header is included.

## Quick examples
- List incidents:
  - Request: `GET /api/v1/incidents` with header `X-API-Key: <your-key>`
  - Response: `200 OK` with an array of incidents or `404 Not Found` if none.

- Create a comment on an incident:
  - Request: `POST /api/v1/incidents/{incidentId}/comment`
  - Headers: `X-API-Key: <key with IncidentEdit>`
  - Body (JSON): `{ "Comment": "Arrived on scene", "EmployeeId": "12345" }`
  - Response: `200 OK` with created comment ID message or `400 Bad Request` on validation failure.

## Building and running
- Solution: `Stack911.sln`
- Project: `src/ApiService/ApiService.csproj`
- Run under IIS Express or self-hosted as configured by your environment.
- Ensure the `config/ApiService_appsettings.json` file exists and contains valid API keys and the ECC data connection string.