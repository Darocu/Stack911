using System.Threading.Tasks;
using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Incidents;

public class UpdateUserDefinedField
{
    private readonly CADManager _cadManager;
    
    public UpdateUserDefinedField(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public async Task UpdateUserDefinedFieldAsync(int incidentId, string userFieldName, string value)
    {
        await Task.Run(() => _cadManager.IncidentActionEngine.SaveIncidentUserDataField(incidentId, userFieldName, value));
    }
}