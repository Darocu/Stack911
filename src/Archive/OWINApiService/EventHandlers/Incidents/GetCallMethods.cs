using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace owinapiservice.EventHandlers.Incidents;

public class GetCallMethods
{
    private readonly CADManager _cadManager;
    
    public GetCallMethods(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public Task<List<MethodOfCallReceived>> Handle(int agencyId)
    {
        var callMethods = _cadManager.IncidentQueryEngine.GetMethodOfCallReceivedsByAgencyID(agencyId);
        return Task.FromResult(callMethods);
    }
}