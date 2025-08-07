using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Radios;

public class DeletePersonnelRadio
{
    private readonly CADManager _cadManager;
    
    public DeletePersonnelRadio(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public void Handle(int personnelRadioId)
    {
        _cadManager.PersonActionEngine.DeletePersonnelRadio(personnelRadioId);
    }
}