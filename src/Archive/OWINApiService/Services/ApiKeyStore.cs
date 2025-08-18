using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using owinapiservice.Models;

namespace owinapiservice.Services;

public class ApiKeyStore
{
    private readonly IConfiguration _configuration;

    public ApiKeyStore(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool TryGetApiKeyInfo(string key, out ApiKeyInfo info)
    {
        var apiKeys = _configuration.GetSection("ApiKeys").Get<List<ApiKeyInfo>>() ?? new List<ApiKeyInfo>();
        info = apiKeys.Find(k => k.Key == key);
        return info != null;
    }
}