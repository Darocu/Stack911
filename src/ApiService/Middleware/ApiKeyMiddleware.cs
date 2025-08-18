using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ApiService.Services;

namespace ApiService.Middleware;

public class ApiKeyHandler : DelegatingHandler
{
    private readonly ApiKeyStore _apiKeyStore;

    public ApiKeyHandler(ApiKeyStore apiKeyStore)
    {
        _apiKeyStore = apiKeyStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri.AbsolutePath;
        if (path == "/" || path.StartsWith("/swagger"))
            return await base.SendAsync(request, cancellationToken);

        if (!request.Headers.TryGetValues("X-API-Key", out var apiKeys) ||
            !_apiKeyStore.TryGetApiKeyInfo(apiKeys.FirstOrDefault(), out var keyInfo))
        {
            return request.CreateResponse(HttpStatusCode.Unauthorized, "API key is missing or invalid.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "ApiKeyUser"),
            new Claim("ApiKey", apiKeys.First())
        };
        claims.AddRange(keyInfo.Permissions.Select(p => new Claim("Permission", p)));
        var identity = new ClaimsIdentity(claims, "ApiKey");
        request.GetRequestContext().Principal = new ClaimsPrincipal(identity);

        return await base.SendAsync(request, cancellationToken);
    }
}