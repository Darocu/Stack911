using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Incidents;

namespace IncidentEventService.EventHandlers;

public class SkydioMarkerHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SkydioMarkerHandler> _logger;
    private const string SkydioUuid = "-83bb-4c9b-97bd-1ae05eb0aeb3";

    public SkydioMarkerHandler(IHttpClientFactory httpClientFactory, ILogger<SkydioMarkerHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
      
    public async Task<bool?> MarkerExistsAsync(string id, int incidentId)
    {
        var client = _httpClientFactory.CreateClient("SkydioApi");
        var url = $"marker/{incidentId}{SkydioUuid}";
        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
            return true;

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        _logger.LogError("[{ID}]: Error retrieving Skydio Marker for IncidentID: {IncidentId}. Status Code: {StatusCode}", 
            id, incidentId, response.StatusCode);

        return null;
    }

    public async Task<bool> CreateMarkerAsync(CADManager cadManager, string id, int incidentId)
    {
        var client = _httpClientFactory.CreateClient("SkydioApi");

        var incident = cadManager.IncidentQueryEngine.GetIncident(incidentId);

        var uuidString = $"{incident.IncidentID}{SkydioUuid}";
        if (!Guid.TryParse(uuidString, out var guid))
        {
            _logger.LogError("[{ID}]: Failed to parse UUID string '{UuidString}' to GUID for IncidentID: {IncidentId}",
                id, uuidString, incidentId);
            return false;
        }

        var payload = BuildMarkerPayload(incident, guid, 1);

        var url = "marker";
        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("[{ID}]: Successfully created Skydio Marker for IncidentID: {IncidentId}",
                id, incidentId);
            return true;
        }

        _logger.LogError("[{ID}]: Failed to create Skydio Marker for IncidentID: {IncidentId}, Status Code: {StatusCode}",
            id, incidentId, response.StatusCode);
        return false;
    }
    
    public async Task<bool> DeleteMarkerAsync(string id, int incidentId)
    {
        var client = _httpClientFactory.CreateClient("SkydioApi");
        var url = $"marker/{incidentId}{SkydioUuid}/delete";
        var response = await client.DeleteAsync(url);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("[{ID}]: Successfully deleted Skydio Marker for IncidentID: {IncidentId}", 
                id, incidentId);
            return true;
        }

        _logger.LogError("[{ID}]: Failed to delete Skydio Marker for IncidentID: {IncidentId}, Status Code: {StatusCode}",
            id, incidentId, response.StatusCode);
        return false;
    }
    
    public async Task<bool> UpdateMarkerAsync(CADManager cadManager, string id, int incidentId)
    {
        var client = _httpClientFactory.CreateClient("SkydioApi");
        var exists = await MarkerExistsAsync(id, incidentId);
        if (exists != true)
        {
            _logger.LogError("[{ID}]: Skydio Marker does not exist for IncidentID: {IncidentId}. Cannot update.",
                id, incidentId);
            return false;
        }

        var currentVersion = await GetCurrentMarkerVersionAsync(client, incidentId);
        if (currentVersion == null)
        {
            _logger.LogError("[{ID}]: Failed to retrieve existing Skydio Marker version for IncidentID: {IncidentId}",
                id, incidentId);
            return false;
        }

        var incident = cadManager.IncidentQueryEngine.GetIncident(incidentId);

        var uuidString = $"{incident.IncidentID}{SkydioUuid}";

        if (!Guid.TryParse(uuidString, out var guid))
        {
            _logger.LogError("[{ID}]: Failed to parse UUID string '{UuidString}' to GUID for IncidentID: {IncidentId}",
                id, uuidString, incidentId);
            return false;
        }

        var payload = BuildMarkerPayload(incident, guid, currentVersion.Value + 1);

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        var postResponse = await client.PostAsync("marker", content);
        if (postResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("[{ID}]: Successfully updated Skydio Marker for IncidentID: {IncidentId}",
                id, incidentId);
            return true;
        }

        _logger.LogError("[{ID}]: Failed to update Skydio Marker for IncidentID: {IncidentId}, Status Code: {StatusCode}",
            id, incidentId, postResponse.StatusCode);
        return false;
    }
    
    private async Task<int?> GetCurrentMarkerVersionAsync(HttpClient client, int incidentId)
    {
        var url = $"marker/{incidentId}{SkydioUuid}";
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var markerJson = await response.Content.ReadAsStringAsync();
        
        using var doc = System.Text.Json.JsonDocument.Parse(markerJson);
        if (doc.RootElement.TryGetProperty("data", out var dataProp) &&
            dataProp.TryGetProperty("marker", out var markerProp) &&
            markerProp.TryGetProperty("version", out var versionProp) &&
            versionProp.TryGetDouble(out var versionDouble))
        {
            return (int)versionDouble;
        }
        
        return null;
    }

    private object BuildMarkerPayload(Incident incident, Guid guid, int version)
    {
        if (incident == null)
            throw new ArgumentNullException(nameof(incident));

        var responseAreaCode = incident.ResponseArea?.Code ?? "Unknown";

        var address = incident.Address ?? "Unknown";

        var problemNatureName = incident.ProblemNatureName ?? "Unknown";

        var priorityName = incident.PriorityName ?? "Unknown";

        var incidentNumber = !string.IsNullOrEmpty(incident.IncidentNumber)
            ? incident.IncidentNumber
            : incident.IncidentID.ToString();

        var priority = incident.Priority?.ToString() ?? "Unknown";
        
        var incidentComments = incident.GetComments() ?? [];
        var descriptionString = incidentComments.Any()
            ? string.Join(Environment.NewLine, incidentComments.Select(c => c.Comment))
            : $"{address} {problemNatureName} {priorityName}";
        
        return new
        {
            area = responseAreaCode,
            description = descriptionString,
            event_time = incident.IncidentTime,
            latitude = incident.Latitude,
            longitude = incident.Longitude,
            marker_details = new
            {
                code = problemNatureName,
                incident_id = incidentNumber,
                priority,
            },
            source_name = "ECC Incident Event Service",
            title = $"{address} {problemNatureName} {priorityName}",
            type = "INCIDENT",
            uuid = guid,
            version
        };
    }
}