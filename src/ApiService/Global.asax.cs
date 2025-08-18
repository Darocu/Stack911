using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ApiService.Middleware;
using ApiService.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ApiService;

public class MvcApplication : System.Web.HttpApplication
{
    private static ILogger _logger;
    private IConfigurationRoot _configuration;
        
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(config =>
        {
            WebApiConfig.Register(config);

            var executableDir = AppDomain.CurrentDomain.BaseDirectory;
            _configuration = StartupConfig.LoadConfiguration(executableDir);
            _logger = StartupConfig.ConfigureSerilog(_configuration, executableDir);

            _logger.Information("Logger initialized at {Path}", executableDir);
            
            var container = DependencyRegistration.Register(config, _configuration, _logger);
            AppDomain.CurrentDomain.ProcessExit += (_, _) => StartupConfig.SafeShutdown(container);

            var apiKeyStore = new ApiKeyStore(_configuration);
            config.MessageHandlers.Add(new ApiKeyHandler(apiKeyStore));
            config.MessageHandlers.Add(new ErrorHandler(_logger));
            config.MessageHandlers.Add(new LoggingHandler(_logger));

            _logger.Information("{Service} started", _configuration["ServiceDisplayName"]);
        });

        AreaRegistration.RegisterAllAreas();
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
    }
}