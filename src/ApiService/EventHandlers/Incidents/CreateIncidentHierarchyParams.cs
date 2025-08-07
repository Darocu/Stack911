using System.Threading.Tasks;
using ApiService.EventHandlers.Hierarchy;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;

public class CreateIncidentHierarchyParams
{
    private readonly CADManager _cadManager;
    private readonly GetAgencyIdByName _getAgencyIdByName;

    public CreateIncidentHierarchyParams(CADManager cadManager, GetAgencyIdByName getAgencyIdByName)
    {
        _cadManager = cadManager;
        _getAgencyIdByName = getAgencyIdByName;
    }
    
    // TODO: Validate agency before proceeding
    internal async Task<CreateIncidentHierarchyParam> CreateIncidentHierarchyParamAsync(string agency)
    { 
        var agencyId = await Task.Run(() => _getAgencyIdByName.GetAgencyIdAsync(agency));

        var hierarchyParams = await Task.Run(() => CreateIncidentHierarchyParam.CreateIncidentHierarchyParamByAgency(_cadManager, agencyId));

        return hierarchyParams;
    }
}