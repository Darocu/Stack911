using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;
using UnitEventService.Models;

namespace UnitEventService.EventHandlers;

public class UnitStatusChangedToTransportHandler
{
    private readonly ServiceSettings _settings;
    private readonly ILogger<UnitStatusChangedToTransport> _logger;
    
    public UnitStatusChangedToTransportHandler(ServiceSettings settings, ILogger<UnitStatusChangedToTransport> logger)
    {
        _settings = settings;
        _logger = logger;
    }
    
    public async Task HandleUnitStatusChangedToTransport(CADManager cadManager, UnitStatusChangedToTransport unitEvent)
    {
        if (_settings.FeatureFlags.EnableCrisisReliefCenterAlerts)
        {
            _logger.LogInformation("[{ID}]: Handling unit status changed to transport event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId}",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
            
            await NotifyCrisisReliefCenter(cadManager, unitEvent);
            
            _logger.LogInformation("[{ID}]: Finished handling unit status changed to transport event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId}",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
        }
        else
        {
            _logger.LogWarning("[{ID}]: Feature flag for crisis relief center alerts is disabled. Skipping handling of unit status changed to transport event for VehicleID: {VehicleId}, ToStatusID: {ToStatusId}.",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.ToStatusID);
        }

        await Task.CompletedTask;
    }

    private async Task NotifyCrisisReliefCenter(CADManager cadManager, UnitStatusChangedToTransport unitEvent)
    {
        if (unitEvent.VehicleID == 0 || unitEvent.FromStatusID == 0 || unitEvent.ToStatusID == 0)
        {
            _logger.LogWarning(
                "[{ID}]: Invalid unit event received. VehicleID: {VehicleId}, FromStatusID: {FromStatusId}, ToStatusID: {ToStatusId}. Event will not be processed.",
                unitEvent.ID, unitEvent.VehicleID, unitEvent.FromStatusID, unitEvent.ToStatusID);
            return;
        }

        if (unitEvent.AgencyID != 2)
        {
            _logger.LogWarning("[{ID}]: Unit event received from agency {AgencyId} which is not supported. Event will not be processed.",
                unitEvent.ID, unitEvent.AgencyID);
            return;
        }
        
        var incidentId = unitEvent.IncidentID;
        
        var incidentAddress = cadManager.IncidentQueryEngine.GetIncidentAddressName(incidentId);
        
        if (incidentAddress != "Crisis Relief Centre")
        {
            _logger.LogWarning("[{ID}]: Unit event received for incident {IncidentId} with address {IncidentAddress} which is not a Crisis Relief Centre. Event will not be processed.",
                unitEvent.ID, incidentId, incidentAddress);
            return;
        }
        
        // TODO: Logic to handle the alert sent to the Crisis Relief Center when a unit status changes to transport.
        
        _logger.LogInformation("[{ID}]: Notifying Crisis Relief Center for VehicleID: {VehicleId}, IncidentID: {IncidentId}",
            unitEvent.ID, unitEvent.VehicleID, unitEvent.IncidentID);
        
        // Simulate notification logic
        await Task.Delay(1000); // Simulating async operation
        
        _logger.LogInformation("[{ID}]: Notification sent to Crisis Relief Center for VehicleID: {VehicleId}, IncidentID: {IncidentId}",
            unitEvent.ID, unitEvent.VehicleID, unitEvent.IncidentID);
    }
}