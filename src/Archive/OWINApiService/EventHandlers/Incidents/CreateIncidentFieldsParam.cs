using System.Threading.Tasks;
using TriTech.Common.Interface;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace owinapiservice.EventHandlers.Incidents;

public class CreateIncidentFieldsParam
{
    private readonly CADManager _cadManager;

    public CreateIncidentFieldsParam(CADManager cadManager)
    {
        _cadManager = cadManager;
    }
        
    // TODO: Validate incidentFieldsParam before returning
    public Task<TriTech.VisiCAD.Incidents.CreateIncidentFieldsParam> Handle(
        string problemName,
        string addressOrLocationOrIntersectionOrLatLonOrAlias,
        string cityName,
        string countyName,
        string stateCodeOrName,
        string crossStreet,
        string zipCode,
        string building,
        string apartment,
        string mapInfo,
        string location,
        string primaryTacChannelName,
        string commandChannelName,
        //int responsePlanId,
        //string responsePlanType,
        string callBackPhone,
        string callerName,
        string callerLocationPhone,
        string callerBuilding,
        CreateIncidentHierarchyParam hierarchyParam)
    {
        var incidentFieldsParam = new TriTech.VisiCAD.Incidents.CreateIncidentFieldsParam(
            _cadManager,
            problemName,
            addressOrLocationOrIntersectionOrLatLonOrAlias,
            cityName,
            countyName,
            stateCodeOrName,
            crossStreet,
            zipCode,
            building,
            apartment,
            mapInfo,
            location,
            primaryTacChannelName,
            commandChannelName,
            0,
            VisiCADDefinition.EnumResponsePlanType.Unassigned,
            callBackPhone,
            callerName,
            callerLocationPhone,
            callerBuilding,
            hierarchyParam);

        return Task.FromResult(incidentFieldsParam);
    }
}