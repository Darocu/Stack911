using TriTech.VisiCAD;

namespace owinapiservice.EventHandlers.Incidents;

public class UpdateCallTakingPerformedBy
{
    private readonly CADManager _cadManager;
    
    public UpdateCallTakingPerformedBy(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public void Handle(string callTakenPerformedBy, int incidentId)
    {
        _cadManager.IncidentActionEngine.UpdateIncidentCalltakingPerformedBy(incidentId, callTakenPerformedBy);
    }
}