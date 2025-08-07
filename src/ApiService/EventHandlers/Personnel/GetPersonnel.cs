using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Personnel;

public class GetPersonnel
{
    private readonly CADManager _cadManager;
    
    public GetPersonnel(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public Task<List<TriTech.VisiCAD.Persons.Personnel>> Handle()
    {
        var personnel = _cadManager.PersonQueryEngine.GetPersonnel();
        return Task.FromResult(personnel);
    }
}