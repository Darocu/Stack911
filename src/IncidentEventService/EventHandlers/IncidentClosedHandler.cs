using System.Threading.Tasks;
using IncidentEventService.Models;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;

namespace IncidentEventService.EventHandlers
{
    public class IncidentClosedHandler
    {
        private readonly ServiceSettings _settings;
        private readonly SkydioMarkerHandler _skydioMarkerHandler;
        private readonly ILogger<IncidentClosedHandler> _logger;

        public IncidentClosedHandler(ServiceSettings settings, SkydioMarkerHandler skydioMarkerHandler, ILogger<IncidentClosedHandler> logger)
        {
            _settings = settings;
            _skydioMarkerHandler = skydioMarkerHandler;
            _logger = logger;
        }

        public async Task HandleIncidentClosed(CADManager cadManager, IncidentClosed incidentEvent)
        {

            if (_settings.FeatureFlags.EnableSkydioMarkerHandler && incidentEvent.AgencyID == 2)
            {
                _logger.LogInformation("[{ID}]: Handling incident closed event for IncidentID: {IncidentId}", incidentEvent.ID, incidentEvent.IncidentID);

                await TriggerDeleteSkydioMarkerAsync(cadManager, incidentEvent.ID, incidentEvent.IncidentID);
                
                _logger.LogInformation("[{ID}]: Finished handling incident closed event for IncidentID: {IncidentId}", incidentEvent.ID,  incidentEvent.IncidentID);
            }
            else if (incidentEvent.AgencyID != 2)
            {
                _logger.LogInformation("[{ID}]: Skipping handling of incident closed event for IncidentID: {IncidentId} as agency is not supported.",
                    incidentEvent.ID, incidentEvent.IncidentID);
            }
            else
            {
                _logger.LogWarning(
                    "[{ID}]: Feature flag for Skydio marker handler is disabled. Skipping handling of incident closed event for IncidentID: {IncidentId}.",
                    incidentEvent.ID, incidentEvent.IncidentID);
            }
        }

        private async Task TriggerDeleteSkydioMarkerAsync(CADManager cadManager, string id, int incidentId)
        {
            _logger.LogInformation("[{ID}]: Incident closed event for IncidentID: {IncidentId} is from Police agency. Processing Skydio event.",
                id, incidentId);

            var skydioMarkerExists = await _skydioMarkerHandler.MarkerExistsAsync(id, incidentId);
            if (skydioMarkerExists == null)
            {
                _logger.LogError("[{ID}]: Stopping processing due to error retrieving Skydio Marker for IncidentID: {IncidentId}.", 
                    id, incidentId);
                return;
            }
            if (!skydioMarkerExists.Value)
            {
                _logger.LogInformation("[{ID}]: Skydio Marker does not exist for IncidentID: {IncidentId}. No action needed.",
                    id, incidentId);
                return;
            }
            _logger.LogInformation("[{ID}]: Skydio Marker exists for IncidentID: {IncidentId}. Deleting marker.",
                id, incidentId);

            await _skydioMarkerHandler.DeleteMarkerAsync(id, incidentId);
        }
    }
}