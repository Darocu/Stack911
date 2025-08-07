using Microsoft.Build.Framework;

namespace ApiService.Models.Incidents;

public class CreateIncidentRequest
{
    [Required]
    public string EmployeeId { get; set; }
    [Required]
    public string Agency { get; set; }
    [Required]
    public string ProblemName { get; set; }
    [Required]
    public string AddressOrLocationOrIntersectionOrLatLonOrAlias { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CityName { get; set; }
    public string CountyName { get; set; }
    public string StateCodeOrName { get; set; }
    public string CrossStreet { get; set; }
    public string ZipCode { get; set; }
    public string Building { get; set; }
    public string Apartment { get; set; }
    public string SecondaryAddress { get; set; }
    public string MapInfo { get; set; }
    public string Location { get; set; }
    public string PrimaryTacChannelName { get; set; }
    public string CommandChannelName { get; set; }
    public int? ResponsePlanId { get; set; }
    public string ResponsePlanType { get; set; }
    public string CallBackPhone { get; set; }
    public string CallerName { get; set; }
    public string CallerLocationPhone { get; set; }
    public string CallerBuilding { get; set; }
    public int? PremiseLocationId { get; set; }
    public string MethodOfCallReceived { get; set; }
    public int? CallerTypeId { get; set; }
    public string CallerAddress { get; set; }
    public string Comment { get; set; }
}
