using System.Collections.Generic;
using System.Threading.Tasks;
using ApiService.Models.Personnel;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Personnel;

public class GetPersonnel
{
    private readonly CADManager _cadManager;
    
    public GetPersonnel(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public async Task<IEnumerable<TriTech.VisiCAD.Persons.Personnel>> GetPersonnelAsync()
    {
        var personnel = _cadManager.PersonQueryEngine.GetPersonnel();
        return personnel;
    }
    
}