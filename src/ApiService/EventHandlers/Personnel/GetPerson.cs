using System;
using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Personnel;

public class GetPerson
{
    private readonly CADManager _cadManager;
    
    public GetPerson(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<TriTech.VisiCAD.Persons.Personnel> GetPersonAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("Employee ID cannot be null or empty.", nameof(employeeId));
        
        return await Task.Run(() => _cadManager.PersonQueryEngine.GetPersonnelByCode(employeeId) ?? _cadManager.PersonQueryEngine.GetPersonnelByName(employeeId));
    }
    
}