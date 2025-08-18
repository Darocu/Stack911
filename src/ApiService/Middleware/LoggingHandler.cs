using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace ApiService.Middleware;

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _logger.Information(
            "Request received: {Method} {Path} from {Host}",
            request.Method,
            request.RequestUri.AbsolutePath,
            request.RequestUri.Host
        );        var response = await base.SendAsync(request, cancellationToken);
        sw.Stop();
        _logger.Information("Request processed in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
        return response;
    }
}