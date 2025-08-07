using System.Collections.Generic;
using System.Threading.Tasks;
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
    
    internal async Task<int> GetAgencyIdAsync(string agencyName)
    {
        var agencyId = await Task.Run(() => _cadManager.HierarchyQueryEngine.GetAgencyIDByName(agencyName));
        
        if (agencyId == 0)
            throw new KeyNotFoundException($"Agency with name '{agencyName}' not found.");
        
        return agencyId;
    }
}