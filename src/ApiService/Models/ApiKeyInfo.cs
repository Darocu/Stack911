using System.Collections.Generic;

namespace ApiService.Models;

public class ApiKeyInfo
{
    public string Key { get; set; }
    public List<string> Permissions { get; set; }
}