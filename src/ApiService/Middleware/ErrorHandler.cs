using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace ApiService.Middleware;

public class ErrorHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public ErrorHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception");
            return request.CreateResponse(HttpStatusCode.InternalServerError, "Internal server error");
        }
    }
}