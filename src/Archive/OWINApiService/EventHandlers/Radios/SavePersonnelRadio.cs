using TriTech.VisiCAD;
using TriTech.VisiCAD.Persons;

namespace owinapiservice.EventHandlers.Radios;

public class SavePersonnelRadio
{
    private readonly CADManager _cadManager;
    
    public SavePersonnelRadio(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public void Handle(PersonnelRadioFields personnelRadioFields)
    {
        _cadManager.PersonActionEngine.SavePersonnelRadio(personnelRadioFields);
    }
}