using System.Collections.Generic;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Units;

namespace owinapiservice.EventHandlers.Units;

public class GetUnits
{
    private readonly CADManager _cadManager;
    
    public GetUnits(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<List<Vehicle>> GetUnitsAsync()
    {
        var vehicles = _cadManager.UnitQueryEngine.GetVehicles();
        return Task.FromResult(vehicles);
    }
}