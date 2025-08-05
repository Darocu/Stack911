using System.Threading.Tasks;
using IncidentEventService.Models;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;

namespace IncidentEventService.EventHandlers
{
    public class PendingIncidentCreatedHandler
    {
        private readonly ServiceSettings _settings;
        private readonly SkydioMarkerHandler _skydioMarkerHandler;
        private readonly ILogger<PendingIncidentCreatedHandler> _logger;

        public PendingIncidentCreatedHandler(ServiceSettings settings, SkydioMarkerHandler skydioMarkerHandler, ILogger<PendingIncidentCreatedHandler> logger)
        {
            _settings = settings;
            _skydioMarkerHandler = skydioMarkerHandler;
            _logger = logger;
        }

        public async Task HandlePendingIncidentCreated(CADManager cadManager, PendingIncidentCreated incidentEvent)
        {
            if (_settings.FeatureFlags.EnableSkydioMarkerHandler && incidentEvent.AgencyID == 2)
            {
                _logger.LogInformation("[{ID}]: Handling pending incident created event for IncidentID: {IncidentId}",
                    incidentEvent.ID, incidentEvent.IncidentID);
                
                await Task.Delay(5000);

                var incident = cadManager.IncidentQueryEngine.GetIncident(incidentEvent.IncidentID);

                if (!incident.IsActive)
                {
                    _logger.LogInformation("[{ID}]: IncidentID: {IncidentId} is not active. Skipping Skydio marker creation.",
                        incidentEvent.ID, incidentEvent.IncidentID);
                }
                else if (_settings.ProblemNatureNameFilter.Contains(incident.ProblemNatureName))
                {
                    _logger.LogInformation("[{ID}]: IncidentID: {IncidentId} Problem Nature Name '{ProblemNatureName}' is filtered. Skipping Skydio marker creation.",
                        incidentEvent.ID, incidentEvent.IncidentID, incident.ProblemNatureName);
                }
                else
                {
                    await TriggerSkydioMarkerCreationAsync(cadManager, incidentEvent.ID, incidentEvent.IncidentID);
                    _logger.LogInformation("[{ID}]: Finished handling pending incident created event for IncidentID: {IncidentId}",
                        incidentEvent.ID, incidentEvent.IncidentID);
                }
            }
            else if (incidentEvent.AgencyID != 2)
            {
                _logger.LogInformation("[{ID}]: Skipping handling of pending incident created event for IncidentID: {IncidentId} as agency is not supported.",
                    incidentEvent.ID, incidentEvent.IncidentID);
            }
            else
            {
                _logger.LogWarning("[{ID}]: Feature flag for Skydio marker handler is disabled. Skipping handling of pending incident created event for IncidentID: {IncidentId}.",
                    incidentEvent.ID, incidentEvent.IncidentID);
            }
        }

        private async Task TriggerSkydioMarkerCreationAsync(CADManager cadManager, string id, int incidentId)
        {
            var skydioMarkerExists = await _skydioMarkerHandler.MarkerExistsAsync(id, incidentId);
            if (skydioMarkerExists == null)
            {
                _logger.LogError("[{ID}]: Stopping processing due to error retrieving Skydio Marker for IncidentID: {IncidentId}.",
                    id, incidentId);
                return;
            }
            if (skydioMarkerExists.Value)
            {
                _logger.LogInformation("[{ID}]: Skydio Marker already exists for IncidentID: {IncidentId}. No action needed.",
                    id, incidentId);
                return;
            }
            _logger.LogInformation("[{ID}]: Skydio Marker does not exist for IncidentID: {IncidentId}. Creating marker.",
                id, incidentId);

            await _skydioMarkerHandler.CreateMarkerAsync(cadManager, id, incidentId);
        }
    }
}