using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Radios;

public class GetPersonnelRadiosByRadioCode
{
    private readonly CADManager _cadManager;
    
    public GetPersonnelRadiosByRadioCode(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<IEnumerable<TriTech.VisiCAD.Persons.PersonnelRadio>> GetPersonnelRadiosByRadioCodeAsync(string radioCode)
    {
        return await Task.Run(() =>_cadManager.PersonQueryEngine.GetPersonnelRadiosByRadioCode(radioCode));
    }
}