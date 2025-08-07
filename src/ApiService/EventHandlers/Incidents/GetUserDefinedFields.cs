using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class GetUserDefinedFields
{
    private readonly CADManager _cadManager;
    
    public GetUserDefinedFields(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<List<UserField>> GetUserDefinedFieldsAsync(int agencyId)
    {
        return await Task.Run(() => _cadManager.IncidentQueryEngine.GetUserFieldsByAgencyID(agencyId));
    }
}