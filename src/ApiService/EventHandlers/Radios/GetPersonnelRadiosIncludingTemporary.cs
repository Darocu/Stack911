using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Radios;

public class GetPersonnelRadiosIncludingTemporary
{
    private readonly CADManager _cadManager;
    
    public GetPersonnelRadiosIncludingTemporary(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<IEnumerable<string>> GetPersonnelRadiosIncludingTemporaryAsync(int personnelId)
    {
        var radios = _cadManager.PersonQueryEngine.GetPersonnelRadiosIncludingTemporary(personnelId);
        return radios;
    }
}