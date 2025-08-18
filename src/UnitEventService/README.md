# UnitEventService

UnitEventService is a long‑running, background worker that subscribes to TriTech VisiCAD events and reacts to unit lifecycle changes. It automates operational tasks such as returning units to Local Area when they become available, tracking stacked incidents, and optionally notifying stakeholders for specific transports.

It is part of the Cincinnati Unified Safety Toolkit (CUST) and runs without an HTTP surface. ApiService exposes REST endpoints; UnitEventService focuses on event‑driven automation for units.

## What it does
- Subscribes to CAD events via TriTech VisiCAD CADManager and handles them asynchronously through a bounded in‑memory queue.
- Processes unit‑related events and triggers handlers:
  - UnitStatusChangedToAvailable → optionally change status to Local Area (LA)
  - UnitStatusChangedToTransport → optionally notify Crisis Relief Center (example placeholder)
  - UnitStackedIncidentsChanged → track/log stacked incident activity
- Applies agency and configurable filters so only supported units are acted on.
- Uses feature flags to turn individual behaviors on/off without code changes.
- Logs operational activity and errors to rolling log files.

## Architecture overview
- Event subscription: Program wires CADManager.CADEventReceived to CadEventService.CadEventReceivedAsync.
- Queueing: CadEventService places incoming business events onto a bounded Channel<object> (capacity 1000) with simple retry on enqueue; a background loop processes them sequentially.
- Dispatch: CadEventService routes each event type to the appropriate handler:
  - UnitStatusChangedToAvailableHandler
  - UnitStatusChangedToTransportHandler
  - UnitStackedIncidentsChangedHandler

## Handlers and behaviors
- UnitStatusChangedToAvailableHandler
  - When enabled via FeatureFlags.EnableAutomaticLAStatus and for supported agency (e.g., AgencyID == 2):
    - Skips if the unit has stacked incidents to avoid interfering with assignments.
    - Skips vehicles listed in settings.InvalidVehicles.
    - If the FromStatusID is listed in settings.InvalidLaStatusIds, waits briefly and verifies the unit is still Available before acting.
    - Determines the unit’s home station and changes status to Local Area using UnitActionEngine.ChangeStatusToLocalArea.
    - Logs the previous/current status names for traceability.
- UnitStatusChangedToTransportHandler
  - When enabled via FeatureFlags.EnableCrisisReliefCenterAlerts:
    - Validates event data, requires supported agency (e.g., AgencyID == 2).
    - Checks incident address for "Crisis Relief Centre"; if matched, performs a notification workflow (currently a placeholder/simulated async operation).
- UnitStackedIncidentsChangedHandler
  - When enabled via FeatureFlags.EnableStackedUnitTracker:
    - Validates event data and resolves the latest stacked incident.
    - Looks up the unit name and incident number, and writes a CAD activity log entry: "Unit Stacked" with details (e.g., "Unit X has been stacked to <incident>").

## Configuration
Configuration is loaded at startup from JSON. Program resolves the config path relative to the executable:
- Development path: ../../../config/uniteventservice_appsettings.json
- Deployed path: ../../config/uniteventservice_appsettings.json

Key settings (ServiceSettings):
- ServiceAccountName: CAD service account used for CADManager.LoginAsServiceAccount
- ServiceDisplayName: Used for log file naming
- FeatureFlags:
  - EnableAutomaticLAStatus: toggle automatic LA status change when a unit becomes available
  - EnableCrisisReliefCenterAlerts: toggle notifications for transport status changes
  - EnableStackedUnitTracker: toggle activity logging for stacked incidents
- InvalidLaStatusIds: set of status IDs that should not immediately trigger LA; the handler waits and verifies the unit remains Available before acting
- InvalidVehicles: list of vehicle identifiers to exclude from automation

Example shape:
{
  "ServiceAccountName": "ECC Unit Service",
  "ServiceDisplayName": "UnitEventService",
  "FeatureFlags": {
    "EnableAutomaticLAStatus": true,
    "EnableCrisisReliefCenterAlerts": false,
    "EnableStackedUnitTracker": true
  },
  "InvalidLaStatusIds": [5, 6],
  "InvalidVehicles": ["TRAINING1", "TESTCAR"]
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
- Binds ServiceSettings from uniteventservice_appsettings.json.
- Creates and logs into a TriTech VisiCAD CADManager using the configured service account.
- Graceful shutdown: a hosted Shutdown service calls CadEventService.StopAsync to flush the queue.

## Operational notes
- Agency filter: Handlers commonly check AgencyID to scope automation (e.g., AgencyID == 2). Adjust per environment.
- Backpressure: inbound events are queued up to 1000; enqueue retries briefly before logging an error.
- Safety checks: the LA status workflow avoids interfering with stacked units and respects invalid vehicles/status IDs.
- Extensibility: additional unit event handlers can be added and wired similarly in CadEventService and Program.

## Building and running
- Solution: Stack911.sln
- Project: src/UnitEventService/UnitEventService.csproj
- Ensure the config/uniteventservice_appsettings.json file exists with valid settings and the logs directory is writable.
- The process must have privileges to use the configured CAD service account.