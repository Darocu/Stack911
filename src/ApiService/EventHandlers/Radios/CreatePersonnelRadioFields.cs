using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Persons;

namespace ApiService.EventHandlers.Radios;

public class CreatePersonnelRadioFields
{
    public async Task<PersonnelRadioFields> CreatePersonnelRadioFieldsAsync(int? personnelRadioId, int personnelId, string radioIdentifier, string radioCode, string radioName)
    {
        var radioFields = new PersonnelRadioFields(personnelRadioId, personnelId, radioIdentifier, radioCode, radioName);
        
        return radioFields;
    }
}