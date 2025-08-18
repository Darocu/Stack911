using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace owinapiservice.EventHandlers.Incidents;

public class GetCallerTypesByAgency
{
    private readonly CADManager _cadManager;
    
    public GetCallerTypesByAgency(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<List<CallerType>> Handle(int agencyId)
    {
        var callerTypes = _cadManager.IncidentQueryEngine.GetCallerTypesByAgencyID(agencyId); 
        return Task.FromResult(callerTypes);
    }
}