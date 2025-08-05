using System.Collections.Generic;

namespace UnitEventService.Models;

public class ServiceSettings
{
    public string ServiceAccountName { get; set; }
    public string ServiceDisplayName { get; set; }
    public FeatureFlags FeatureFlags { get; set; } = new();
    public HashSet<int> InvalidLaStatusIds { get; set; } = [];
    public HashSet<string> InvalidVehicles { get; set; } = [];
}

public class FeatureFlags
{
    public bool EnableAutomaticLAStatus { get; set; }
    public bool EnableCrisisReliefCenterAlerts { get; set; }
    public bool EnableStackedUnitTracker { get; set; }
}