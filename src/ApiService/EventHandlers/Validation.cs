using System;
using System.Linq;
using System.Threading.Tasks;
using ApiService.EventHandlers.Hierarchy;
using ApiService.EventHandlers.Incidents;
using ApiService.EventHandlers.Personnel;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers;

public class Validation
{
    private readonly GetIncident _getIncident;
    private readonly GetPerson _getPerson;
    private readonly GetCallMethods _getCallMethods;
    private readonly GetCallerTypesByAgency _getCallerTypesByAgency;
    private readonly GetUserDefinedFields _getUserDefinedFields;
    private readonly GetAgencyIdByName _getAgencyIdByName;
    
    public Validation(
        GetIncident getIncident,
        GetPerson getPerson,
        GetCallMethods getCallMethods,
        GetCallerTypesByAgency getCallerTypesByAgency,
        GetUserDefinedFields getUserDefinedFields,
        GetAgencyIdByName getAgencyIdByName)
    {
        _getIncident = getIncident;
        _getPerson = getPerson;
        _getCallMethods = getCallMethods;
        _getCallerTypesByAgency = getCallerTypesByAgency;
        _getUserDefinedFields = getUserDefinedFields;
        _getAgencyIdByName = getAgencyIdByName;
    }

    public async Task<Incident> ValidateIncidentAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            return null;

        if (int.TryParse(incidentId, out var incId))
            return await _getIncident.GetIncidentAsync(incId);
        else
            return await _getIncident.GetIncidentAsync(incidentId);
    }

    public async Task<TriTech.VisiCAD.Persons.Personnel> ValidatePersonnelAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            return null;

        return await _getPerson.GetPersonAsync(employeeId);
    }
    
    public async Task<MethodOfCallReceived> ValidateCallMethodAsync(int agencyId, string callMethod)
    {
        if (string.IsNullOrWhiteSpace(callMethod))
            throw new ArgumentException("Call method cannot be null or empty.", nameof(callMethod));

        var callMethods = await _getCallMethods.GetCallMethodsAsync(agencyId);

        var matched = callMethods
            .FirstOrDefault(m => string.Equals(m.Name.ToString(), callMethod, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            throw new ArgumentException($"Invalid call method: {callMethod}", nameof(callMethod));

        return matched;
    }
    
    public async Task<CallerType> ValidateCallerTypeAsync(int agencyId, string callerTypeName)
    {
        if (string.IsNullOrWhiteSpace(callerTypeName))
            throw new ArgumentException("Caller type cannot be null or empty.", nameof(callerTypeName));

        var callerType = await _getCallerTypesByAgency.GetCallerTypeAsync(agencyId);
        
        var matched = callerType
            .FirstOrDefault(ct => string.Equals(ct.Name, callerTypeName, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            throw new ArgumentException($"Invalid caller type: {callerTypeName}", nameof(callerTypeName));

        return matched;
    }
    
    public async Task<UserField> ValidateUserDefinedFieldAsync(int agencyId, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("User defined field name cannot be null or empty.", nameof(fieldName));

        var userDefinedFields = await _getUserDefinedFields.GetUserDefinedFieldsAsync(agencyId);

        var matched = userDefinedFields
            .FirstOrDefault(f => string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            throw new ArgumentException($"Invalid user defined field: {fieldName}", nameof(fieldName));

        return matched;
    }
    
    public async Task<int> ValidateAgencyAsync(string agency)
    {
        if (string.IsNullOrWhiteSpace(agency))
            throw new ArgumentException("Agency name cannot be null or empty.", nameof(agency));

        var agencyId = await _getAgencyIdByName.GetAgencyIdAsync(agency);

        if (agencyId <= 0)
            throw new ArgumentException($"Invalid agency name: {agency}", nameof(agency));

        return agencyId;
    }
        
        
}