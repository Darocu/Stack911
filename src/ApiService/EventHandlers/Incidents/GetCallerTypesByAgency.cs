using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class GetCallerTypesByAgency
{
    private readonly CADManager _cadManager;
    
    public GetCallerTypesByAgency(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<List<CallerType>> GetCallerTypeAsync(int agencyId)
    {
        _cadManager.IncidentQueryEngine.GetCallerTypesByAgencyID(agencyId);
        return await Task.Run(() => _cadManager.IncidentQueryEngine.GetCallerTypesByAgencyID(agencyId));
    }
}