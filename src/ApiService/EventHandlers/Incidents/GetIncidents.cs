using System.Collections.Generic;
using System.Linq;
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
    
    public async Task<List<Incident>> GetAllIncidentsAsync()
    {
        return await Task.Run(() => _cadManager.IncidentQueryEngine.GetIncidentsByActiveCalls());
    }
}