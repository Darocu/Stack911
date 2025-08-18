using System;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace owinapiservice.EventHandlers.Incidents;
// TODO: Add logic to get incidents older than 30 days.
public class GetIncident
{
    private readonly CADManager _cadManager;
    
    public GetIncident(CADManager cadManager)
    {
        _cadManager = cadManager;
    }

    public Task<Incident> Handle(string incidentNumber)
    {
        var incidentId = _cadManager.IncidentQueryEngine.GetIncidentIDByIncidentNumber(incidentNumber);

        if (!incidentId.HasValue)
            throw new ArgumentException($"Incident not found for number {incidentNumber}", nameof(incidentNumber));
    
        return Handle(incidentId.Value);
    }
    
    public Task<Incident> Handle(int incidentId)
    {
        var incident = _cadManager.IncidentQueryEngine.GetIncident(incidentId);

        if (incident == null)
            throw new ArgumentNullException(nameof(incidentId), "Incident not found");
        
        return Task.FromResult(incident);
    }
}