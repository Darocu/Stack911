using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class UpdateCallerType
{
    private readonly CADManager _cadManager;
    
    public UpdateCallerType(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task UpdateCallerTypeAsync(int incidentId, CallerType callerType)
    {
        await Task.Run(() => _cadManager.IncidentActionEngine.UpdateIncidentCallerType(incidentId, callerType.CallerTypeID));
    }
}