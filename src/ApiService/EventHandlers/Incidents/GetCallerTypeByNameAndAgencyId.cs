using System;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class GetCallerTypeByNameAndAgencyId
{
    private readonly CADManager _cadManager;
    
    public GetCallerTypeByNameAndAgencyId(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<CallerType> GetCallerTypeAsync(string callerType, int agencyId)
    {
        return await Task.Run(() => _cadManager.IncidentQueryEngine.GetCallerTypeByNameAndAgencyID(callerType, agencyId));
    }
}