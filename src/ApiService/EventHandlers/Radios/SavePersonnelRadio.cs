using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Persons;

namespace ApiService.EventHandlers.Radios;

public class SavePersonnelRadio
{
    private readonly CADManager _cadManager;
    
    public SavePersonnelRadio(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task SavePersonnelRadioAsync(PersonnelRadioFields personnelRadioFields)
    {
        await Task.Run(() => _cadManager.PersonActionEngine.SavePersonnelRadio(personnelRadioFields));
    }
}