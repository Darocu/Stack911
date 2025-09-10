using TriTech.VisiCAD;

namespace owinapiservice.EventHandlers.Incidents;

public class UpdateCallMethod
{
    private readonly CADManager _cadManager;
    
    public UpdateCallMethod(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public void Handle(int incidentId, string callMethod)
    {
        _cadManager.IncidentActionEngine.UpdateIncidentMethodOfCallRcvd(incidentId, callMethod);
    }
}