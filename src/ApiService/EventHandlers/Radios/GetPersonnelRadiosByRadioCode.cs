using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Persons;

namespace ApiService.EventHandlers.Radios;

public class GetPersonnelRadiosByRadioCode
{
    private readonly CADManager _cadManager;
    
    public GetPersonnelRadiosByRadioCode(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<List<PersonnelRadio>> Handle(string radioCode)
    {
        var radios = _cadManager.PersonQueryEngine.GetPersonnelRadiosByRadioCode(radioCode);
        return Task.FromResult(radios);
    }
}