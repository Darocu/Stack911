using System.ComponentModel.DataAnnotations;

namespace ApiService.Models.Incidents;

public class CreateCommentRequest
{
    [Required]
    public string EmployeeId { get; set; }
    [Required]
    public string Comment { get; set; }
}