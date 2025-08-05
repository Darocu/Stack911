using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;
using UnitEventService.Models;

namespace UnitEventService.EventHandlers;

public class UnitStackedIncidentsChangedHandler
{
    private readonly ServiceSettings _settings;
    private readonly ILogger<UnitStackedIncidentsChangedHandler> _logger;
    
    public UnitStackedIncidentsChangedHandler(ServiceSettings settings,
        ILogger<UnitStackedIncidentsChangedHandler>  logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task HandleUnitStackedIncidentsChanged(CADManager cadManager, UnitStackedIncidentsChanged unitEvent)
    {
        if (_settings.FeatureFlags.EnableStackedUnitTracker)
        {
            _logger.LogInformation("[{ID}]: Handling unit stacked incidents changed event for VehicleID: {VehicleId}, StackedIncidents: {StackedIncidents}",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.StackedIncidents.Length);
            
            await StackedUnitTracker(cadManager, unitEvent);
            
            _logger.LogInformation("[{ID}]: Finished handling unit stacked incidents changed event for VehicleID: {VehicleId}, StackedIncidents: {StackedIncidents}",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.StackedIncidents.Length);
        }
        else
        {
            _logger.LogWarning("[{ID}]: Feature flag for stacked unit tracker is disabled. Skipping handling of unit stacked incidents changed event for VehicleID: {VehicleId}.",
                unitEvent.ID, unitEvent.VehicleID);
        }
    }
    
    private async Task StackedUnitTracker(CADManager cadManager, UnitStackedIncidentsChanged unitEvent)
    {
        if (unitEvent.VehicleID == 0)
        {
            _logger.LogWarning(
                "[{ID}]: Invalid unit event received. VehicleID: {VehicleId}, StackedIncidents: {StackedIncidents}. Event will not be processed.",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.StackedIncidents);
            return;
        }
        
        var lastItem = unitEvent.StackedIncidents.Last(); // Get the latest stacked incident
        
        if (lastItem == null)
        {
            _logger.LogWarning("[{ID}]: No stacked incidents found for vehicle ID {VehicleId}. Event will not be processed.",
                unitEvent.ID, unitEvent.VehicleID);
            return;
        }
        var unitName = cadManager.UnitQueryEngine.GetUnitName(unitEvent.UnitID); // Get the units name
        
        if (string.IsNullOrEmpty(unitName))
        {
            _logger.LogWarning("[{ID}]: Unit name not found for vehicle ID {VehicleId}. Event will not be processed.",
                unitEvent.ID, unitEvent.VehicleID);
            return;
        }
        
        var incidentNumber = cadManager.IncidentQueryEngine.GetIncidentNumber(lastItem.IncidentID); // Get the incident number
        
        if (string.IsNullOrEmpty(incidentNumber))
        {
            _logger.LogWarning("[{ID}]: Incident number not found for incident ID {IncidentId}. Event will not be processed.",
                unitEvent.ID, lastItem.IncidentID);
            return;
        }

        cadManager.GeneralActionEngine.AddActivityLogEntry(lastItem.IncidentID, unitEvent.VehicleID, // Add the activity log entry
            "Unit Stacked", $@"Unit {unitName} has been stacked to {incidentNumber}");
        
        await Task.CompletedTask;
    }
}