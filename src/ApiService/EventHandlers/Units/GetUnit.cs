using System.Threading;
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
    
    public async Task<Vehicle> GetUnitAsync(string unitId)
    {
        return await Task.Run(() => _cadManager.UnitQueryEngine.GetVehicleByName(unitId));
    }
}