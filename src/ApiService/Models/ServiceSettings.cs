using System.Collections.Generic;

namespace ApiService.Models
{
    public class ServiceSettings
    {
        public string ServiceAccountName { get; set; }
        public string ServiceDisplayName { get; set; }  
        public string EccDataConnectionString { get; set; }
        public ApiKeys ApiKeys { get; set; }
    }

    public class ApiKeys
    {
        public string Key { get; set; }
        public HashSet<string> Permissions { get; set; }
    }
}