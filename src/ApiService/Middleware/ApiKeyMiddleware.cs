using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiService.Services;
using Microsoft.Owin;

namespace ApiService.Middleware;

public class ApiKeyMiddleware : OwinMiddleware
{
    private readonly ApiKeyStore _apiKeyStore;

    public ApiKeyMiddleware(OwinMiddleware next, ApiKeyStore apiKeyStore) : base(next)
    {
        _apiKeyStore = apiKeyStore;
    }

    public override async Task Invoke(IOwinContext context)
    {
        var requestPath = context.Request.Path.ToString();

        if (requestPath.Equals("/", StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            requestPath.StartsWith("/swagger/ui", StringComparison.OrdinalIgnoreCase))
        {
            await Next.Invoke(context);
            return;
        }

        var apiKey = context.Request.Headers["X-API-Key"];
        if (string.IsNullOrWhiteSpace(apiKey) || !_apiKeyStore.TryGetApiKeyInfo(apiKey, out var keyInfo))
        {
            context.Response.StatusCode = 401;
            context.Response.ReasonPhrase = "Invalid or missing API key";
            await context.Response.WriteAsync("API key is missing or invalid.");
            return;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "ApiKeyUser"),
            new Claim("ApiKey", apiKey)
        };
        claims.AddRange(keyInfo.Permissions.Select(p => new Claim("Permission", p)));
        var identity = new ClaimsIdentity(claims, "ApiKey");
        context.Request.User = new ClaimsPrincipal(identity);

        await Next.Invoke(context);
    }
}