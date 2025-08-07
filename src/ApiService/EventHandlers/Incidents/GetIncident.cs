using System;
using System.Threading.Tasks;
using ApiService.Models;
using ApiService.Models.Incidents;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace ApiService.EventHandlers.Incidents;
// TODO: Add logic to get incidents older than 30 days.
public class GetIncident
{
    private readonly CADManager _cadManager;
    
    public GetIncident(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public async Task<Incident> GetIncidentAsync(string incidentNumber)
    {
        var incidentId = _cadManager.IncidentQueryEngine.GetIncidentIDByIncidentNumber(incidentNumber);

        if (!incidentId.HasValue)
            throw new ArgumentException($"Incident not found for number {incidentNumber}", nameof(incidentNumber));
        
        return await GetIncidentAsync(incidentId.Value);
    }
    
    public Task<Incident> GetIncidentAsync(int incidentId)
    {
        var incident = _cadManager.IncidentQueryEngine.GetIncident(incidentId);

        if (incident == null)
            throw new ArgumentNullException(nameof(incidentId), "Incident not found");
        
        return Task.FromResult(incident);
    }
}