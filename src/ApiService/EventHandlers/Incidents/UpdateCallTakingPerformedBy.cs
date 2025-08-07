using System;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Incidents;

public class UpdateCallTakingPerformedBy
{
    private readonly CADManager _cadManager;
    
    public UpdateCallTakingPerformedBy(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    // TODO: Validate callTakenPerformedBy and IncidentId before proceeding
    public async Task UpdateCallTakingPerformedByAsync(string callTakenPerformedBy, int incidentId)
    {
        await Task.Run(() => _cadManager.IncidentActionEngine.UpdateIncidentCalltakingPerformedBy(incidentId,
            callTakenPerformedBy));
    }
}