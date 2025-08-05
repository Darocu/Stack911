using System.Collections.Generic;

namespace IncidentEventService.Models;

public class ServiceSettings
{
    public string ServiceAccountName { get; set; }
    public string ServiceDisplayName { get; set; }
    public string ProxyUrl { get; set; }
    public string SkydioApiUrl { get; set; }
    public string SkydioActiveEnvironment { get; set; }
    public Dictionary<string, string> SkydioApiKeys { get; set; } = new();
    public FeatureFlags FeatureFlags { get; set; } = new();

    public HashSet<string> ProblemNatureNameFilter { get; set; } = new();
    
    public string SkydioApiKey =>
        SkydioApiKeys != null && SkydioApiKeys.TryGetValue(SkydioActiveEnvironment, out var key) ? key : null;
}

public class FeatureFlags
{
    public bool EnableIncidentTrackingHandler { get; set; }
    public bool EnableSkydioMarkerHandler { get; set; }
}