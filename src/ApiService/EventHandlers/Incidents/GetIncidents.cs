using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class GetIncidents
{
    private readonly CADManager _cadManager;
    
    public GetIncidents(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<List<Incident>> Handle()
    {
        var incidents = _cadManager.IncidentQueryEngine.GetIncidentsByActiveCalls();
        return Task.FromResult(incidents);
    }
}