using System;

namespace ApiService.Models.Positions;

public class PositionResponse
{
    public int MachineId { get; set; }
    public string MachineName { get; set; }
    public int CadUserId { get; set; }
    public string EmployeeId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string ControlledSectorName { get; set; }
    public DateTime ReportedTime { get; set; }
}