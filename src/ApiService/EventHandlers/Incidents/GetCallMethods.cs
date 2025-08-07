using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class GetCallMethods
{
    private readonly CADManager _cadManager;
    
    public GetCallMethods(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public async Task<List<MethodOfCallReceived>> GetCallMethodsAsync(int agencyId)
    {
        if (agencyId <= 0)
            throw new ArgumentException("Agency ID must be greater than zero.", nameof(agencyId));

        return await Task.Run(() => _cadManager.IncidentQueryEngine.GetMethodOfCallReceivedsByAgencyID(agencyId));
    }
}