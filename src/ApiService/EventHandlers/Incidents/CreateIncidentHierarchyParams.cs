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
    public Task<CreateIncidentHierarchyParam> Handle(string agency)
    {
        var agencyId = _getAgencyIdByName.Handle(agency);
        var hierarchyParams = CreateIncidentHierarchyParam.CreateIncidentHierarchyParamByAgency(_cadManager, agencyId);
        return Task.FromResult(hierarchyParams);
    }
}