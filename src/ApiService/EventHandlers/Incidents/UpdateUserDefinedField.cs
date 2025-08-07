using TriTech.VisiCAD;

namespace ApiService.EventHandlers.Incidents;

public class UpdateUserDefinedField
{
    private readonly CADManager _cadManager;
    
    public UpdateUserDefinedField(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
    
    public void Handle(int incidentId, string userFieldName, string value)
    {
        _cadManager.IncidentActionEngine.SaveIncidentUserDataField(incidentId, userFieldName, value);
    }
}