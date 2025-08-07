using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Radios;

public class DeletePersonnelRadio
{
    private readonly CADManager _cadManager;
    
    public DeletePersonnelRadio(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task DeletePersonnelRadioAsync(int personnelRadioId)
    {
        await Task.Run(() => _cadManager.PersonActionEngine.DeletePersonnelRadio(personnelRadioId));
    }
}