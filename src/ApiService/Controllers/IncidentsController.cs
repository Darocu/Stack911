using System.Threading.Tasks;
using System.Web.Http;
using ApiService.Authorization;
using ApiService.EventHandlers;
using ApiService.EventHandlers.Incidents;
using ApiService.Models.Incidents;
using TriTech.VisiCAD.Incidents;

namespace ApiService.Controllers;

[Authorize]
[RoutePrefix("api/v1/incidents")]
public class IncidentsController : ApiController
{
    private readonly CreateIncident _createIncident;
    private readonly CreateComment _createComment;
    private readonly GetIncident _getIncident;
    private readonly GetIncidents _getIncidents;
    private readonly UpdateCallTakingPerformedBy _updateCallTakingPerformedBy;
    private readonly UpdateCallMethod _updateCallMethod;
    private readonly UpdateCallerType _updateCallerType;
    private readonly UpdateUserDefinedField _updateUserDefinedField;
    private readonly GetCallMethods _getCallMethods;
    private readonly GetCallerTypesByAgency _getCallerTypesByAgency;
    private readonly GetUserDefinedFields _getUserDefinedFields;
    private readonly Validation _validation;

    public IncidentsController(
        CreateIncident createIncident,
        CreateComment createComment,
        GetIncident getIncident,
        GetIncidents getIncidents,
        UpdateCallTakingPerformedBy updateCallTakingPerformedBy,
        UpdateCallMethod updateCallMethod,
        UpdateCallerType updateCallerType,
        UpdateUserDefinedField updateUserDefinedField,
        GetCallMethods getCallMethods,
        GetCallerTypesByAgency getCallerTypesByAgency,
        GetUserDefinedFields getUserDefinedFields,
        Validation validation)
    {
        _createIncident = createIncident;
        _createComment = createComment;
        _getIncident = getIncident;
        _getIncidents = getIncidents;
        _updateCallTakingPerformedBy = updateCallTakingPerformedBy;
        _updateCallMethod = updateCallMethod;
        _updateCallerType = updateCallerType;
        _updateUserDefinedField = updateUserDefinedField;
        _getCallMethods = getCallMethods;
        _getCallerTypesByAgency = getCallerTypesByAgency;
        _getUserDefinedFields = getUserDefinedFields;
        _validation = validation;
    }
    
    [HttpGet]
    [Route("")]
    [RequirePermission("IncidentView")]
    public async Task<IHttpActionResult> GetAllActive()
    {
        var incidents = await _getIncidents.Handle();
        
        if (incidents == null || !incidents.Any()) return NotFound();

        return Ok(incidents);
    }

    [HttpGet]
    [Route("{incidentId}")] 
    [RequirePermission("IncidentView")]
    public async Task<IHttpActionResult> GetIncident(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            return BadRequest("Incident ID is required.");

        Incident incident;

        if (int.TryParse(incidentId, out var incId))
            incident = await _getIncident.Handle(incId);
        else
            incident = await _getIncident.Handle(incidentId);

        if (incident == null)
            return NotFound();

        return Ok(incident);
    }
    
    [HttpGet]
    [Route("userdefinedfields/{agencyName}")]
    [RequirePermission("IncidentView")]
    public async Task<IHttpActionResult> GetUserDefinedFields(string agencyName)
    {
        if (string.IsNullOrEmpty(agencyName))
            return BadRequest("Agency ID is required.");

        // Validate agency
        var agencyId = await _validation.ValidateAgency(agencyName);
        if (agencyId <= 0) return BadRequest($"Agency '{agencyName}' is not valid.");

        var userDefinedFields = await _getUserDefinedFields.Handle(agencyId);

        return Ok(userDefinedFields);
    }
    
    [HttpGet]
    [Route("callmethods/{agencyName}")]
    [RequirePermission("IncidentView")]
    public async Task<IHttpActionResult> GetCallMethods(string agencyName)
    {
        if (string.IsNullOrEmpty(agencyName))
            return BadRequest("Agency ID is required.");

        // Validate agency
        var agencyId = await _validation.ValidateAgency(agencyName);
        if (agencyId <= 0) return BadRequest($"Agency '{agencyName}' is not valid.");
        
        var callMethods = await _getCallMethods.Handle(agencyId);
        
        return Ok(callMethods);
    }
    
    [HttpGet]
    [Route("callertypes/{agencyName}")]
    [RequirePermission("IncidentView")]
    public async Task<IHttpActionResult> GetCallerType(string agencyName)
    {
        if (string.IsNullOrEmpty(agencyName))
            return BadRequest("Agency ID is required.");

        // Validate agency
        var agencyId = await _validation.ValidateAgency(agencyName);
        if (agencyId <= 0) return BadRequest($"Agency '{agencyName}' is not valid.");
        
        var callerTypes = await _getCallerTypesByAgency.Handle(agencyId);
        
        return Ok(callerTypes);
    }
    
    [HttpPost]
    [Route("create")] 
    [RequirePermission("IncidentEdit")]
    // TODO: TEST
    public async Task<IHttpActionResult> Create([FromBody] CreateIncidentRequest payload)
    {
        if (payload == null)
            return BadRequest("Incident data is required.");

        var incidentId = await _createIncident.Handle(payload);
        
        if (incidentId <= 0)
            return BadRequest("Failed to create incident.");
        
        return Ok($"Created incident: {incidentId}");
    }
    
    [HttpPost]
    [Route("{incidentId}/comment")] 
    [RequirePermission("IncidentEdit")]
    // TODO: TEST
    public async Task<IHttpActionResult> CreateComment(string incidentId, [FromBody] CreateCommentRequest payload)
    {
        if (string.IsNullOrEmpty(incidentId))
            return BadRequest("Incident ID is required.");
        
        if (payload.Comment == null || string.IsNullOrWhiteSpace(payload.Comment) || payload.EmployeeId == null || string.IsNullOrWhiteSpace(payload.EmployeeId))
            return BadRequest("Comment data is required.");
        
        // Validate incident and personnel
        var (incident, personnel) = (await _validation.ValidateIncident(incidentId), await _validation.ValidatePersonnelAsync(payload.EmployeeId));
        if (incident == null) return BadRequest($"Incident ID '{incidentId}' is not valid.");
        if (personnel == null) return BadRequest($"Employee ID '{payload.EmployeeId}' is not valid.");
        
        // Create Comment
        var commentId = await _createComment.Handle(payload.Comment, incident.ID, payload.EmployeeId);
        
        if (commentId <= 0) return BadRequest("Failed to create comment on incident.");
        
        return Ok($"Created comment {commentId} on incident: {incidentId}");
    }
    
    [HttpPost]
    [Route("{incidentId}/calltakenby/{employeeId}")]
    [RequirePermission("IncidentEdit")]
    // TODO: TEST
    public async Task<IHttpActionResult> UpdateCallTakenBy(string incidentId, string employeeId)
    {
        if (string.IsNullOrEmpty(incidentId))
            return BadRequest("Incident ID is required.");
        
        if (employeeId == null || string.IsNullOrWhiteSpace(employeeId))
            return BadRequest("Call taken by is required.");

        // Validate incident and personnel
        var (incident, personnel) = (await _validation.ValidateIncident(incidentId), await _validation.ValidatePersonnelAsync(employeeId));
        if (incident == null) return BadRequest($"Incident ID '{incidentId}' is not valid.");
        if (personnel == null) return BadRequest($"Employee ID '{employeeId}' is not valid.");
        
        // Update Call Taken By
        _updateCallTakingPerformedBy.Handle(personnel.Name, incident.ID);
        
        return Ok($"Updated call taken by for incident ID: {incidentId} to employee: {personnel.Name}");
    }
    
    [HttpPost]
    [Route("{incidentId}/callmethod/{callMethod}")]
    [RequirePermission("IncidentEdit")]
    // TODO: TEST
    public async Task<IHttpActionResult> UpdateCallMethod(string incidentId, string callMethod)
    {
        if (string.IsNullOrEmpty(incidentId))
            return BadRequest("Incident ID is required.");
        
        if (callMethod == null || string.IsNullOrWhiteSpace(callMethod))
            return BadRequest("Call taken by is required.");

        // Validate incident and callMethod
        var incident = await _validation.ValidateIncident(incidentId);
        if (incident == null) return BadRequest($"Incident ID '{incidentId}' is not valid.");

        var validCallMethod = await _validation.ValidateCallMethod(incident.AgencyID, callMethod);
        if (validCallMethod == null) return BadRequest($"Call method '{callMethod}' is not valid for agency ID: {incident.AgencyID}.");
        
        // Update Call Method
        _updateCallMethod.Handle( incident.ID, validCallMethod.Name);
        
        return Ok($"Updated call method for incident ID: {incidentId} to '{validCallMethod.Name}'");
    }
    
    [HttpPost]
    [Route("{incidentId}/callertype/{callerType}")]
    [RequirePermission("IncidentEdit")]
    // TODO: TEST
    public async Task<IHttpActionResult> UpdateCallerType(string incidentId, string callerType) // TODO: JObject data for more complex data
    {
        if (string.IsNullOrEmpty(incidentId))
            return BadRequest("Incident ID is required.");
        
        if (callerType == null || string.IsNullOrWhiteSpace(callerType))
            return BadRequest("Call taken by is required.");

        // Validate incident and callMethod
        var incident = await _validation.ValidateIncident(incidentId);
        if (incident == null) return BadRequest($"Incident ID '{incidentId}' is not valid.");
        
        var validCallerType = await _validation.ValidateCallerType(incident.AgencyID, callerType);
        if (validCallerType == null) return BadRequest($"Caller type '{callerType}' is not valid for agency ID: {incident.AgencyID}.");
        
        // Update Caller Type
        _updateCallerType.Handle(incident.ID, validCallerType);
        
        return Ok($"Updated caller type for incident ID: {incidentId} to '{validCallerType.Name}'");
    }
    
    [HttpPost]
    [Route("{incidentId}/userdefinedfield")]
    [RequirePermission("IncidentEdit")]
    // TODO: TEST
    public async Task<IHttpActionResult> UpdateUserDefinedField(string incidentId, [FromBody] UpdateUserDefinedFieldRequest payload) // TODO: JObject data for more complex data
    {
        if (string.IsNullOrEmpty(incidentId))
            return BadRequest("Incident ID is required.");

        if (payload.UserDefinedField == null || string.IsNullOrWhiteSpace(payload.UserDefinedField))
            return BadRequest("User defined field is required.");

        // Validate incident and callMethod
        var incident = await _validation.ValidateIncident(incidentId);
        if (incident == null) return BadRequest($"Incident ID '{incidentId}' is not valid.");
        
        // Validate User Defined Field
        var validUdf = await _validation.ValidateUserDefinedField(incident.AgencyID, payload.UserDefinedField);
        if (validUdf == null) return BadRequest($"User defined field '{payload.UserDefinedField}' is not valid for agency ID: {incident.AgencyID}.");
        
        // Update User Defined Field
        _updateUserDefinedField.Handle(incident.ID, validUdf.Name, payload.Value);
        
        return Ok($"Updated user defined field '{validUdf.Name}' for incident ID: {incidentId} to '{payload.Value}'");
    }
}