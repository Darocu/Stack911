using System;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Incidents;

public class UpdateCallMethod
{
    private readonly CADManager _cadManager;
    
    public UpdateCallMethod(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task UpdateCallMethodAsync(int incidentId, string callMethod)
    {
        await Task.Run(() =>  _cadManager.IncidentActionEngine.UpdateIncidentMethodOfCallRcvd(incidentId, callMethod));
    }
}