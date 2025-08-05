using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IncidentEventService.EventHandlers;
using IncidentEventService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TriTech.VisiCAD;

namespace IncidentEventService
{
    internal abstract class Program
    {
        public static async Task Main(string[] args)
        {
            var executableDir = AppContext.BaseDirectory;

            var configFilePath = ResolveConfigFilePath(executableDir);
            var tempConfig = LoadConfiguration(configFilePath);

            var serviceDisplayName = tempConfig["ServiceDisplayName"] ?? "IncidentEventService";

            var logsFolderPath = ResolveLogsFolderPath(executableDir);
            var logFilePath = Path.Combine(logsFolderPath, $"{serviceDisplayName}Log-.txt");

            ConfigureSerilog(logFilePath);

            try
            {
                Log.Information("Starting {ServiceDisplayName}", serviceDisplayName);

                var host = Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureAppConfiguration((_, config) =>
                    {
                        config.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        var serviceSettings = new ServiceSettings();
                        context.Configuration.Bind(serviceSettings);

                        services.AddSingleton(serviceSettings);
                        services.Configure<ServiceSettings>(context.Configuration);

                        services.AddSingleton(sp =>
                        {
                            var settings = sp.GetRequiredService<ServiceSettings>();
                            var cadManager = new CADManager();
                            cadManager.LoginAsServiceAccount(settings.ServiceAccountName);
                            return cadManager;
                        });

                        services.AddSingleton<CadEventService>();
                        services.AddHostedService<Shutdown>();

                        services.AddHttpClient("SkydioApi", (sp, client) =>
                        {
                            var settings = sp.GetRequiredService<ServiceSettings>();
                            client.BaseAddress = new Uri(settings.SkydioApiUrl);
                            client.DefaultRequestHeaders.Add("Authorization", settings.SkydioApiKey);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        })
                        .ConfigurePrimaryHttpMessageHandler(sp =>
                        {
                            var settings = sp.GetRequiredService<ServiceSettings>();
                            return new HttpClientHandler
                            {
                                UseProxy = true,
                                Proxy = !string.IsNullOrWhiteSpace(settings.ProxyUrl)
                                    ? new WebProxy(settings.ProxyUrl)
                                    : null
                            };
                        });

                        services.AddSingleton<IncidentAddressChangedHandler>();
                        services.AddSingleton<IncidentClosedHandler>();
                        services.AddSingleton<IncidentProblemChangedHandler>();
                        services.AddSingleton<PendingIncidentCreatedHandler>();
                        services.AddSingleton<SkydioMarkerHandler>();

                        Log.Information("SkydioActiveEnvironment: {SkydioActiveEnvironment}", serviceSettings.SkydioActiveEnvironment);
                        Log.Information("SkydioApiUrl: {SkydioApiUrl}", serviceSettings.SkydioApiUrl);
                        Log.Information("EnableSkydioMarkerHandler: {EnableSkydioMarkerHandler}", serviceSettings.FeatureFlags.EnableSkydioMarkerHandler);
                        Log.Information("Filtered Problem Nature Names: {ProblemNatureNameFilter}", string.Join(", ", serviceSettings.ProblemNatureNameFilter));
                    })
                    .Build();

                var serviceProvider = host.Services;

                var cadManager = serviceProvider.GetRequiredService<CADManager>();
                var cadEventService = serviceProvider.GetRequiredService<CadEventService>();

                cadManager.CADEventReceived += cadEventService.CadEventReceivedAsync;

                cadManager.StartReceivingEvents();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static string ResolveConfigFilePath(string executableDir)
        {
            var devConfigPath = Path.GetFullPath(Path.Combine(executableDir, @"..\..\..\..\config\incidenteventservice_appsettings.json"));
            var deployedConfigPath = Path.Combine(executableDir, @"..\..\..\config\incidenteventservice_appsettings.json");

            if (File.Exists(devConfigPath))
                return devConfigPath;
            if (File.Exists(deployedConfigPath))
                return deployedConfigPath;

            throw new FileNotFoundException("Could not find config file", devConfigPath);
        }

        private static IConfiguration LoadConfiguration(string configFilePath)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(configFilePath, optional: false, reloadOnChange: true)
                .Build();
        }

        private static string ResolveLogsFolderPath(string executableDir)
        {
            var devLogsPath = Path.GetFullPath(Path.Combine(executableDir, @"..\..\..\..\logs"));
            var deployedLogsPath = Path.Combine(executableDir, @"..\..\..\logs");

            if (Directory.Exists(devLogsPath))
                return devLogsPath;
            if (Directory.Exists(deployedLogsPath))
                return deployedLogsPath;

            var fallback = Path.Combine(executableDir, "logs");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        private static void ConfigureSerilog(string logFilePath)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 15,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }
    }

    public class Shutdown(CadEventService cadEventService) : IHostedService
    {
        private readonly CadEventService _cadEventService = cadEventService ?? throw new ArgumentNullException(nameof(cadEventService));

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _cadEventService.StopAsync();
        }
    }
}