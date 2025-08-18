using System;
using System.Threading.Tasks;
using ApiService.Models.Incidents;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Geography;

namespace ApiService.EventHandlers.Incidents;
// TODO: Request should go from Controller where it validates the API key and then calls this service
// TODO: Before running this service it should take the employee ID and authorize them in CAD
// TODO: Workflow: Endpoint --> Authentication Middleware--> CAD Authorization Middleware (Emp ID) --> HandleAsync
public class CreateIncident
{
    private readonly CADManager _cadManager;
    private readonly CreateIncidentFieldsParam _createIncidentFieldsParam;
    private readonly CreateIncidentHierarchyParams _createIncidentHierarchyParams;
    private readonly CreateComment _createComment;

    public CreateIncident(CADManager cadManager, 
        CreateIncidentFieldsParam createIncidentFieldsParam,
        CreateIncidentHierarchyParams createIncidentHierarchyParams,
        CreateComment createComment)
    {
        _cadManager = cadManager;
        _createIncidentFieldsParam = createIncidentFieldsParam;
        _createIncidentHierarchyParams = createIncidentHierarchyParams;
        _createComment = createComment;
    }

    // TODO: Test this method thoroughly, especially the geographic validation and incident creation logic
    public async Task<int> HandleAsync(CreateIncidentRequest payload)
    {
        var incidentHierarchyParams = await _createIncidentHierarchyParams.Handle(payload.Agency);

        if (incidentHierarchyParams == null)
            throw new ArgumentNullException(nameof(incidentHierarchyParams), "Incident hierarchy parameters cannot be null.");

        var incidentFieldParams = await _createIncidentFieldsParam.Handle(
            payload.ProblemName,
            payload.AddressOrLocationOrIntersectionOrLatLonOrAlias,
            payload.CityName,
            payload.CountyName,
            payload.StateCodeOrName,
            payload.CrossStreet,
            payload.ZipCode,
            payload.Building,
            payload.Apartment,
            payload.MapInfo,
            payload.Location,
            payload.PrimaryTacChannelName,
            payload.CommandChannelName,
            payload.CallBackPhone,
            payload.CallerName,
            payload.CallerLocationPhone,
            payload.CallerBuilding,
            incidentHierarchyParams
        );

        if (incidentFieldParams == null)
            throw new ArgumentNullException(nameof(incidentFieldParams), "Incident field parameters cannot be null.");
        
        var incidentId = 0;

        // TODO: For some reason when this errors it stops processing the request
        await Task.Run(() => _cadManager.IncidentActionEngine.CreateIncidentOnlyIfGeoValidateExactMatch(incidentFieldParams, out incidentId));

        // TODO: LAT/LON did not work properly
        if (incidentId == 0 &&
            !string.IsNullOrEmpty(incidentFieldParams.AddressOrLocationOrIntersectionOrLatLonOrAlias) &&
            payload.Latitude != 0 &&
            payload.Longitude != 0)
        {
            var address = new Address(incidentFieldParams.AddressOrLocationOrIntersectionOrLatLonOrAlias);
            var geographicPoint = new GeographicPoint(payload.Latitude, payload.Longitude);
            var geographicLocation = new GeographicLocation(address, geographicPoint);
            await Task.Run(() => _cadManager.IncidentActionEngine.CreateIncident(incidentFieldParams, geographicLocation, payload.ResponsePlanId ?? 0, out incidentId));
        }

        if (incidentId == 0)
            await Task.Run(() => _cadManager.IncidentActionEngine.CreateIncident(incidentFieldParams, out incidentId));

        if (incidentId == 0)
            throw new Exception("Incident creation failed, incident ID is 0.");

        if (!string.IsNullOrEmpty(payload.Comment))
            await _createComment.Handle(payload.Comment, incidentId, payload.EmployeeId);

        return incidentId;
    }
}
