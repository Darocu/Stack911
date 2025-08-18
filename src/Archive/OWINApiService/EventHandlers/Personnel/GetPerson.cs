using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace owinapiservice.EventHandlers.Personnel;

public class GetPerson
{
    private readonly CADManager _cadManager;
    
    public GetPerson(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<TriTech.VisiCAD.Persons.Personnel> Handle(string employeeId)
    {
        var person = _cadManager.PersonQueryEngine.GetPersonnelByCode(employeeId)
                     ?? _cadManager.PersonQueryEngine.GetPersonnelByName(employeeId);
        return Task.FromResult(person);
    }
}