using System;
using System.Web.Http;
using Microsoft.Extensions.Configuration;
using Owin;
using owinapiservice.Config;
using Serilog;

namespace owinapiservice;

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

        AppDomain.CurrentDomain.ProcessExit += (_, _) => StartupConfig.SafeShutdown(container);

        app.UseAutofacMiddleware(container);
        app.UseAutofacWebApi(config);

        MiddlewareConfig.Configure(app, _logger, _configuration);

        app.UseWebApi(config);
        _logger.Information("{Service} started", _configuration["ServiceDisplayName"]);
    }
}