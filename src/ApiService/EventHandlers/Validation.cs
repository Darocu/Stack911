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

    public Task<Incident> ValidateIncident(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            return Task.FromResult<Incident>(null);

        return int.TryParse(incidentId, out var incId)
            ? _getIncident.Handle(incId) : _getIncident.Handle(incidentId);
    }

    public Task<TriTech.VisiCAD.Persons.Personnel> ValidatePersonnelAsync(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            return Task.FromResult<TriTech.VisiCAD.Persons.Personnel>(null);

        return _getPerson.Handle(employeeId);
    }

    public Task<MethodOfCallReceived> ValidateCallMethod(int agencyId, string callMethod)
    {
        if (string.IsNullOrWhiteSpace(callMethod))
            throw new ArgumentException("Call method cannot be null or empty.", nameof(callMethod));

        var callMethodsTask = _getCallMethods.Handle(agencyId);
        var callMethods = callMethodsTask.Result;

        var matched = callMethods
            .FirstOrDefault(m => string.Equals(m.Name.ToString(), callMethod, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            throw new ArgumentException($"Invalid call method: {callMethod}", nameof(callMethod));

        return Task.FromResult(matched);
    }

    public Task<CallerType> ValidateCallerType(int agencyId, string callerTypeName)
    {
        if (string.IsNullOrWhiteSpace(callerTypeName))
            throw new ArgumentException("Caller type cannot be null or empty.", nameof(callerTypeName));

        var callerTypeTask = _getCallerTypesByAgency.Handle(agencyId);
        var callerType = callerTypeTask.Result;

        var matched = callerType
            .FirstOrDefault(ct => string.Equals(ct.Name, callerTypeName, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            throw new ArgumentException($"Invalid caller type: {callerTypeName}", nameof(callerTypeName));

        return Task.FromResult(matched);
    }

    public Task<UserField> ValidateUserDefinedField(int agencyId, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("User defined field name cannot be null or empty.", nameof(fieldName));

        var userDefinedFieldsTask = _getUserDefinedFields.Handle(agencyId);
        var userDefinedFields = userDefinedFieldsTask.Result;

        var matched = userDefinedFields
            .FirstOrDefault(f => string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            throw new ArgumentException($"Invalid user defined field: {fieldName}", nameof(fieldName));

        return Task.FromResult(matched);
    }

    public Task<int> ValidateAgency(string agency)
    {
        if (string.IsNullOrWhiteSpace(agency))
            throw new ArgumentException("Agency name cannot be null or empty.", nameof(agency));

        var agencyId = _getAgencyIdByName.Handle(agency);

        if (agencyId <= 0)
            throw new ArgumentException($"Invalid agency name: {agency}", nameof(agency));

        return Task.FromResult(agencyId);
    }
}