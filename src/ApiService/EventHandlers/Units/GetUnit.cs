using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Units;

namespace ApiService.EventHandlers.Units;

public class GetUnit
{
    private readonly CADManager _cadManager;
    
    public GetUnit(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public Task<Vehicle> GetUnitAsync(string unitId)
    {
        var vehicle = _cadManager.UnitQueryEngine.GetVehicleByName(unitId);
        return Task.FromResult(vehicle);
    }
}