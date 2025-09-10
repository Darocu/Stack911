using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace owinapiservice.EventHandlers.Radios;

public class GetPersonnelRadiosIncludingTemporary
{
    private readonly CADManager _cadManager;
    
    public GetPersonnelRadiosIncludingTemporary(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<List<string>> Handle(int personnelId)
    {
        var radios = _cadManager.PersonQueryEngine.GetPersonnelRadiosIncludingTemporary(personnelId);
        return Task.FromResult(radios);
    }
}