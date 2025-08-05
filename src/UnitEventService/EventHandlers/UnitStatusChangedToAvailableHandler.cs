using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;
using TriTech.VisiCAD.Units.Actions.StatusChange;
using UnitEventService.Models;

namespace UnitEventService.EventHandlers;

public class UnitStatusChangedToAvailableHandler
{
    private readonly ServiceSettings _settings;
    private readonly ILogger<UnitStatusChangedToAvailableHandler> _logger;
    
    public UnitStatusChangedToAvailableHandler(ServiceSettings settings,
        ILogger<UnitStatusChangedToAvailableHandler> logger)
    {
        _settings = settings;
        _logger = logger;
    }
    
    public async Task HandleUnitStatusChangedToAvailable(CADManager cadManager, UnitStatusChangedToAvailable unitEvent)
    {
        if (_settings.FeatureFlags.EnableAutomaticLAStatus && unitEvent.AgencyID == 2)
        {
            _logger.LogInformation("[{ID}]: Handling unit status changed to available event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId}",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
            
            await ChangeStatusToLocalAreaAsync(cadManager, unitEvent);
            
            _logger.LogInformation("[{ID}]: Finished handling unit status changed to available event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId}",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
        }
        else if (unitEvent.AgencyID != 2)
        {
            _logger.LogInformation("[{ID}]: Skipping handling of unit status changed to available event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId} as agency is not supported.",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
        }
        else
        {
            _logger.LogWarning("[{ID}]: Feature flag for automatic LA status change is disabled. Skipping handling of unit status changed to available event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId}.",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
        }
    }
    
    private async Task ChangeStatusToLocalAreaAsync(CADManager cadManager, UnitStatusChangedToAvailable unitEvent)
    {
        if (unitEvent.VehicleID == 0 || unitEvent.ToStatusID == 0)
        {
            _logger.LogWarning(
                "[{ID}]: Invalid unit event received. VehicleID: {VehicleId}, FromStatusID: {FromStatusId}, ToStatusID: {ToStatusId}. Event will not be processed.",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.FromStatusID, unitEvent.ToStatusID);
            return;
        }
        
        // Get the target unit from the CAD system
        var targetUnit = cadManager.UnitQueryEngine.GetVehicle(unitEvent.VehicleID);
        
        // Check if the unit is not null
        if (targetUnit == null)
        {
            _logger.LogWarning("[{ID}]: Unit with ID {VehicleId} not found in CAD system.",
                unitEvent.ID, unitEvent.VehicleID);
            return;
        }
        
        // If unit is stacked return
        // This fixes an issue where if a unit is stacked and then put into the LA status
        // it will not be automatically dispatched to the next call. There is a 30-second
        // countdown that starts when a unit moves the AV, after 30 seconds it assigns them.
        if (targetUnit.VehicleHasStackedIncident())
        {
            _logger.LogWarning("[{ID}]: Unit {VehicleId} is stacked and will not be processed.",
                unitEvent.ID, targetUnit.VehicleID);
            return;
        }
        
        // Check if the vehicle is invalid
        if (_settings.InvalidVehicles.Contains(targetUnit.ToString()))
        {
            _logger.LogWarning("[{ID}]: Vehicle {VehicleId} is marked as invalid and will not be processed.",
                unitEvent.ID, targetUnit.ToString());
            return;
        }
        
        // Check if the units from status is invalid
        if (_settings.InvalidLaStatusIds.Contains(unitEvent.FromStatusID))
        {
            await Task.Delay(1000); // Simulate a delay of 1 second
            var currentStatus = cadManager.UnitQueryEngine.GetVehicle(unitEvent.VehicleID);
            if (currentStatus.StatusID != 1) return; // If the status is not available, exit the method
        }
        // Get unit home station
        var homeStation = await Task.Run(() => cadManager.UnitQueryEngine.GetVehicleHomeStationIDByVehicleID(targetUnit.VehicleID));

        // Change the unit status to Local Area
        var localAreaParameters = new ChangeStatusToLocalAreaParameters(cadManager, targetUnit.VehicleID, homeStation, "Automated status change performed by the ECC API Service.")
        {
            ActionTime = DateTime.Now
        };
        
        //localAreaParameters.ValidateParameters();
        
        await Task.Run(() => cadManager.UnitActionEngine.ChangeStatusToLocalArea(localAreaParameters));
        // Log the status change
        var previousStatusName = cadManager.UnitQueryEngine.GetStatusDescription(unitEvent.FromStatusID);
        var statusName = cadManager.UnitQueryEngine.GetStatusDescription(unitEvent.ToStatusID);
        _logger.LogInformation("[{ID}]: Unit {UnitName} (ID: {VehicleId}) status changed from '{PreviousStatus}' to '{CurrentStatus}' to LA.",
            unitEvent.ID, targetUnit.Name, targetUnit.VehicleID, previousStatusName, statusName);
    }
}