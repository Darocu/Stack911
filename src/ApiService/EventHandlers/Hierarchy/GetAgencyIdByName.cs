using System.Collections.Generic;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Hierarchy;
// TODO: Create Endpoint to get agency ID by name
public class GetAgencyIdByName
{
    private readonly CADManager _cadManager;
    
    public GetAgencyIdByName(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public int Handle(string agencyName)
    {
        var agencyId = _cadManager.HierarchyQueryEngine.GetAgencyIDByName(agencyName);
        
        if (agencyId == 0)
            throw new KeyNotFoundException($"Agency with name '{agencyName}' not found.");
        
        return agencyId;
    }
}