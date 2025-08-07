using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Units;

namespace ApiService.EventHandlers.Units;

public class GetUnits
{
    private readonly CADManager _cadManager;
    
    public GetUnits(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task<List<Vehicle>> GetUnitsAsync()
    {
        return await Task.Run(() => _cadManager.UnitQueryEngine.GetVehicles());
    }
}