using System.Threading.Tasks;
using IncidentEventService.Models;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;

namespace IncidentEventService.EventHandlers;

public class IncidentAddressChangedHandler
{
    private readonly ServiceSettings _settings;
    private readonly SkydioMarkerHandler _skydioMarkerHandler;
    private readonly ILogger<IncidentAddressChangedHandler> _logger;

    public IncidentAddressChangedHandler(ServiceSettings settings,
        SkydioMarkerHandler skydioMarkerHandler,
        ILogger<IncidentAddressChangedHandler> logger)
    {
        _settings = settings;
        _skydioMarkerHandler = skydioMarkerHandler;
        _logger = logger;
    }

    public async Task HandleIncidentAddressChanged(CADManager cadManager, IncidentAddressChanged incidentEvent)
    {

        if (_settings.FeatureFlags.EnableSkydioMarkerHandler && incidentEvent.AgencyID == 2)
        {
            _logger.LogInformation("[{ID}]: Handling incident address changed event for IncidentID: {IncidentId}", incidentEvent.ID, incidentEvent.IncidentID);

            await Task.Delay(5000);

            var incident = cadManager.IncidentQueryEngine.GetIncident(incidentEvent.IncidentID);

            if (!incident.IsActive)
            {
                _logger.LogInformation("[{ID}]: IncidentID: {IncidentId} is not active. Skipping Skydio marker creation.", incidentEvent.ID, incidentEvent.IncidentID);
            }
            else if (_settings.ProblemNatureNameFilter.Contains(incident.ProblemNatureName))
            {
                _logger.LogInformation("[{ID}]: IncidentID: {IncidentId} Problem Nature Name '{ProblemNatureName}' is filtered. Skipping Skydio marker creation.",
                    incidentEvent.ID, incidentEvent.IncidentID, incident.ProblemNatureName);
            }
            else
            {
                await TriggerUpdateSkydioMarkerAsync(cadManager, incidentEvent.ID, incidentEvent.IncidentID);
                _logger.LogInformation("[{ID}]: Finished handling incident address changed event for IncidentID: {IncidentId}", incidentEvent.ID, incidentEvent.IncidentID);
            }
        }
        else if (incidentEvent.AgencyID != 2)
        {
            _logger.LogInformation("[{ID}]: Skipping handling of incident address changed event for IncidentID: {IncidentId} as agency is not supported.",
                incidentEvent.ID, incidentEvent.IncidentID);
        }
        else
        {
            _logger.LogWarning("[{ID}]: Feature flag for Skydio marker handler is disabled. Skipping handling of incident address changed event for IncidentID: {IncidentId}.",
                incidentEvent.ID, incidentEvent.IncidentID);
        }
    }

    private async Task TriggerUpdateSkydioMarkerAsync(CADManager cadManager, string id, int incidentId)
    {
        _logger.LogInformation(
            "[{ID}]: Incident address changed event for IncidentID: {IncidentId} is from Police agency. Processing Skydio event.",
            id, incidentId);

        var skydioMarkerExists = await _skydioMarkerHandler.MarkerExistsAsync(id, incidentId);

        if (skydioMarkerExists == null)
        {
            _logger.LogError(
                "[{ID}]: Stopping processing due to error retrieving Skydio Marker for IncidentID: {IncidentId}.",
                id, incidentId);
            return;
        }

        if (!skydioMarkerExists.Value)
        {
            _logger.LogInformation(
                "[{ID}]: Skydio Marker does not exist for IncidentID: {IncidentId}. No action needed.",
                id, incidentId);
            return;
        }

        _logger.LogInformation(
            "[{ID}]: Skydio Marker found for IncidentID: {IncidentId}. Updating marker.",
            id, incidentId);

        await _skydioMarkerHandler.UpdateMarkerAsync(cadManager, id, incidentId);
    }
}