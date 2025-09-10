using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace owinapiservice.EventHandlers.Incidents;

public class GetUserDefinedFields
{
    private readonly CADManager _cadManager;
    
    public GetUserDefinedFields(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<List<UserField>> Handle(int agencyId)
    {
        var userDefinedFields = _cadManager.IncidentQueryEngine.GetUserFieldsByAgencyID(agencyId);
        return Task.FromResult(userDefinedFields);
    }
}