using System.Threading.Tasks;
using IncidentEventService.Models;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;

namespace IncidentEventService.EventHandlers
{
    public class IncidentProblemChangedHandler
    {
        private readonly ServiceSettings _settings;
        private readonly SkydioMarkerHandler _skydioMarkerHandler;
        private readonly ILogger<IncidentProblemChangedHandler> _logger;

        public IncidentProblemChangedHandler(ServiceSettings settings, SkydioMarkerHandler skydioMarkerHandler, ILogger<IncidentProblemChangedHandler> logger)
        {
            _settings = settings;
            _skydioMarkerHandler = skydioMarkerHandler;
            _logger = logger;
        }

        public async Task HandleIncidentProblemChanged(CADManager cadManager, IncidentProblemChanged incidentEvent)
        {
            

            if (_settings.FeatureFlags.EnableIncidentTrackingHandler)
            {
                _logger.LogInformation("[{ID}]: Handling incident problem changed event for IncidentID: {IncidentId}",
                    incidentEvent.ID, incidentEvent.IncidentID);
                
                await TriggerIncidentTrackingAsync(cadManager, incidentEvent);
                
                _logger.LogInformation("[{ID}]: Finished handling incident problem changed event for IncidentID: {IncidentId}",
                    incidentEvent.ID,incidentEvent.IncidentID);
            }
            else
            {
                _logger.LogWarning("[{ID}]: Feature flag for incident tracking handler is disabled. Skipping handling of incident problem changed event for IncidentID: {IncidentId}.",
                    incidentEvent.ID, incidentEvent.IncidentID);
            }
        }

        private async Task TriggerIncidentTrackingAsync(CADManager cadManager, IncidentProblemChanged incidentEvent)
        {
            var problemNatureCode = cadManager.IncidentQueryEngine.GetProblemNatureCode(incidentEvent.FromProblemID);
            var problemNatureName = cadManager.IncidentQueryEngine.GetProblemNatureName(incidentEvent.FromProblemID);

            if (string.IsNullOrEmpty(problemNatureCode) || string.IsNullOrEmpty(problemNatureName))
            {
                _logger.LogWarning("[{ID}]: Problem nature code or name is empty for ProblemID: {ProblemId}. Event will not be processed.",
                    incidentEvent.ID, incidentEvent.FromProblemID);
                return;
            }

            _logger.LogInformation("[{ID}]: Problem nature changed from '{FromProblemName}' to '{ToProblemName}' for IncidentID: {IncidentId}.",
                incidentEvent.ID, incidentEvent.FromProblemID, incidentEvent.ToProblemID, incidentEvent.IncidentID);
            
            var incidentTrackingField = cadManager.IncidentQueryEngine.GetIncidentUserDataFieldByIncidentIDandFieldName(incidentEvent.IncidentID, "Problem Tracker");

            var newIncidentTrackingValue = incidentTrackingField == null
                ? $"{problemNatureCode}"
                : $"{problemNatureCode}, {incidentTrackingField.FieldValue}";

            cadManager.IncidentActionEngine.SaveIncidentUserDataField(incidentEvent.IncidentID, "Problem Tracker", newIncidentTrackingValue);

            await Task.CompletedTask;
        }
    }
}