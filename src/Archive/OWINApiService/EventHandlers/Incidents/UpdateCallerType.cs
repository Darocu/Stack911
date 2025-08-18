using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace owinapiservice.EventHandlers.Incidents;

public class UpdateCallerType
{
    private readonly CADManager _cadManager;
    
    public UpdateCallerType(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public void Handle(int incidentId, CallerType callerType)
    {
        _cadManager.IncidentActionEngine.UpdateIncidentCallerType(incidentId, callerType.CallerTypeID);
    }
}