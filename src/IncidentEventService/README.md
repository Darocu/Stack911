# IncidentEventService

IncidentEventService is a long‑running, background service that subscribes to TriTech VisiCAD events and reacts to specific incident lifecycle changes. It enriches and forwards select incident information to external systems (e.g., Skydio) and updates CAD user data fields to support downstream workflows.

It is part of the Cincinnati Unified Safety Toolkit (CUST) and runs without an HTTP surface. The companion ApiService exposes REST endpoints, while IncidentEventService focuses on event‑driven automation.

## What it does
- Subscribes to CAD events via TriTech VisiCAD CADManager and handles them asynchronously through a bounded in‑memory queue.
- Processes incident‑related events and triggers handlers:
  - PendingIncidentCreated → create Skydio marker if enabled and criteria matched
  - IncidentAddressChanged → update Skydio marker if it exists
  - IncidentClosed → delete Skydio marker if it exists
  - IncidentProblemChanged → append to an incident tracking user data field ("Problem Tracker")
- Applies agency and problem‑nature filters so only supported incidents are acted on.
- Uses feature flags to turn individual behaviors on/off without code changes.
- Logs operational activity and errors to rolling log files.

## Architecture overview
- Event subscription: Program wires CADManager.CADEventReceived to CadEventService.CadEventReceivedAsync.
- Queueing: CadEventService places incoming business events onto a bounded Channel<object> (capacity 1000) with simple retry on enqueue; a background loop processes them sequentially.
- Dispatch: CadEventService routes each event type to the appropriate handler:
  - IncidentAddressChangedHandler
  - IncidentClosedHandler
  - IncidentProblemChangedHandler
  - PendingIncidentCreatedHandler
- External integration: SkydioMarkerHandler encapsulates HTTP calls to the Skydio API for create/update/delete and lookup operations.

## Handlers and behaviors
- PendingIncidentCreatedHandler
  - Optional 5‑second delay to allow CAD to settle; checks incident active state and ProblemNatureName filters; creates a Skydio marker if one does not already exist.
- IncidentAddressChangedHandler
  - On police agency incidents and feature enabled, verifies marker existence and updates it with latest location/details.
- IncidentClosedHandler
  - Deletes an existing Skydio marker when the incident is closed (if feature enabled and agency supported).
- IncidentProblemChangedHandler
  - Builds a running list of prior problem codes in the "Problem Tracker" Incident User Data field using IncidentActionEngine/QueryEngine.

## Configuration
Configuration is loaded at startup from JSON. Program resolves the config path relative to the executable:
- Development path: ../../../config/incidenteventservice_appsettings.json
- Deployed path: ../../config/incidenteventservice_appsettings.json

Key settings (ServiceSettings):
- ServiceAccountName: CAD service account used for CADManager.LoginAsServiceAccount
- ServiceDisplayName: Used for log file naming
- ProxyUrl: Optional HTTP proxy for outbound Skydio calls
- SkydioApiUrl: Base URL of Skydio integration API
- SkydioActiveEnvironment: Key used to select a value from SkydioApiKeys
- SkydioApiKeys: Map of environment → API key (Authorization header value)
- FeatureFlags:
  - EnableIncidentTrackingHandler: toggle incident problem tracking updates
  - EnableSkydioMarkerHandler: toggle all Skydio marker operations
- ProblemNatureNameFilter: set of problem nature names to ignore for Skydio creation/update

Example shape:
{
  "ServiceAccountName": "ECC Incident Service",
  "ServiceDisplayName": "IncidentEventService",
  "ProxyUrl": "http://proxy.local:8080",
  "SkydioApiUrl": "https://skydio.example/api/",
  "SkydioActiveEnvironment": "prod",
  "SkydioApiKeys": { "dev": "Bearer DEV_KEY", "prod": "Bearer PROD_KEY" },
  "FeatureFlags": {
    "EnableIncidentTrackingHandler": true,
    "EnableSkydioMarkerHandler": true
  },
  "ProblemNatureNameFilter": ["TEST CALL", "TRAINING"]
}

## Logging and error handling
- Serilog is configured at startup (Program.ConfigureSerilog) to write daily rolling files to the logs folder.
- Log file name pattern: {ServiceDisplayName}Log-YYYYMMDD.txt
- Logs folder resolution:
  - Development: ../../../logs relative to executable
  - Deployed: ../../logs relative to executable
  - Fallback: ./logs created next to executable

## Hosting and runtime
- Built as a .NET Generic Host worker process; no web server.
- Registers HTTP client named "SkydioApi" with base address and Authorization header drawn from ServiceSettings.
- Optionally configures a WebProxy if ProxyUrl is provided.
- Graceful shutdown: a hosted Shutdown service calls CadEventService.StopAsync to flush the queue.

## Operational notes
- Agency filter: Many handlers check incidentEvent.AgencyID == 2 before acting; adjust as needed for your environment.
- Marker identity: SkydioMarkerHandler constructs a deterministic UUID from IncidentID plus a constant suffix; the same key is used for all operations.
- Idempotency:
  - Create only occurs if MarkerExistsAsync returns false.
  - Update requires marker to exist.
  - Delete skips when no marker exists.
- Backpressure: inbound events are queued up to 1000; enqueue retries briefly before logging an error.

## Building and running
- Solution: Stack911.sln
- Project: src/IncidentEventService/IncidentEventService.csproj
- Ensure the config/incidenteventservice_appsettings.json file exists with valid settings and the logs directory is writable.
- The process must have privileges to use the configured CAD service account.
