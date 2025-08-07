using System;
using System.Web.Http;
using ApiService.Config;
using Microsoft.Extensions.Configuration;
using Owin;
using Serilog;


namespace ApiService;

public class Startup
{
    private IConfigurationRoot _configuration;
    private ILogger _logger;
    
    public void Configuration(IAppBuilder app)
    {
        var executableDir = AppDomain.CurrentDomain.BaseDirectory;
        _configuration = StartupConfig.LoadConfiguration(executableDir);
        _logger = StartupConfig.ConfigureSerilog(_configuration, executableDir);

        var config = new HttpConfiguration();
        config.MapHttpAttributeRoutes();
        SwaggerConfig.Register(config);

        var container = DependencyRegistration.Register(config, _configuration, _logger);

        AppDomain.CurrentDomain.ProcessExit += (_, __) => StartupConfig.SafeShutdown(container);

        app.UseAutofacMiddleware(container);
        app.UseAutofacWebApi(config);

        MiddlewareConfig.Configure(app, _logger, _configuration);

        app.UseWebApi(config);
        _logger.Information("{Service} started", _configuration["ServiceDisplayName"]);
    }
}