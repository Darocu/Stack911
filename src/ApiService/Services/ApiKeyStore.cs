using System.Collections.Concurrent;
using System.Collections.Generic;
using ApiService.Models;
using Microsoft.Extensions.Configuration;

namespace ApiService.Services;

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