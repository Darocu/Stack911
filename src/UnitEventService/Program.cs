using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;
using UnitEventService.Models;

namespace UnitEventService
{
    internal abstract class Program
    {
        public static async Task Main(string[] args)
        {
            var executableDir = AppContext.BaseDirectory;

            var configFilePath = ResolveConfigFilePath(executableDir);
            var tempConfig = LoadConfiguration(configFilePath);

            var serviceDisplayName = tempConfig["ServiceDisplayName"] ?? "UnitEventService";

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

                        services.AddSingleton<UnitStatusChangedToAvailable>();
                        services.AddSingleton<UnitStatusChangedToTransport>();
                        services.AddSingleton<UnitStackedIncidentsChanged>();
                        
                        Log.Information("Filtered Status IDs: {StatusIDs}", string.Join(", ", serviceSettings.InvalidLaStatusIds));
                        Log.Information("Filtered Vehicles: {Vehicles}", string.Join(", ", serviceSettings.InvalidVehicles));
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
            var devConfigPath = Path.GetFullPath(Path.Combine(executableDir, @"..\..\..\..\config\uniteventservice_appsettings.json"));
            var deployedConfigPath = Path.Combine(executableDir, @"..\..\..\config\uniteventservice_appsettings.json");

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