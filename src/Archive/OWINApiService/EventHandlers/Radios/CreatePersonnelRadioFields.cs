using System.Threading.Tasks;
using TriTech.VisiCAD.Persons;

namespace owinapiservice.EventHandlers.Radios;

public class CreatePersonnelRadioFields
{
    public Task<PersonnelRadioFields> Handle(int? personnelRadioId, int personnelId, string radioIdentifier, string radioCode, string radioName)
    {
        var result = new PersonnelRadioFields(personnelRadioId, personnelId, radioIdentifier, radioCode, radioName);
        return Task.FromResult(result);
    }
}