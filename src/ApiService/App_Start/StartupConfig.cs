using System.IO;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace ApiService
{
    public static class StartupConfig
    {
        internal static IConfigurationRoot LoadConfiguration(string executableDir)
        {
            var configFilePath = ResolveConfigFilePath(executableDir);
            return new ConfigurationBuilder()
                .AddJsonFile(configFilePath, optional: false, reloadOnChange: true)
                .Build();
        }

        internal static ILogger ConfigureSerilog(IConfigurationRoot configuration, string executableDir)
        {
            var serviceDisplayName = configuration["ServiceDisplayName"] ?? "UnitEventService";
            var logsFolderPath = ResolveLogsFolderPath(executableDir);
            var logFilePath = Path.Combine(logsFolderPath, $"{serviceDisplayName}Log-.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 15,
                    shared: true,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            return Log.Logger;
        }
        
        private static string ResolveConfigFilePath(string executableDir)
        {
            var devConfigPath = Path.GetFullPath(Path.Combine(executableDir, @"..\..\config\apiservice_appsettings.json"));
            var deployedConfigPath = Path.Combine(executableDir, @"..\config\apiservice_appsettings.json");
            if (File.Exists(devConfigPath))
                return devConfigPath;
            if (File.Exists(deployedConfigPath))
                return deployedConfigPath;
            throw new FileNotFoundException("Could not find config file", devConfigPath);
        }

        private static string ResolveLogsFolderPath(string executableDir)
        {
            var devLogsPath = Path.GetFullPath(Path.Combine(executableDir, @"..\..\logs"));
            var deployedLogsPath = Path.Combine(executableDir, @"..\logs");
            if (Directory.Exists(devLogsPath))
                return devLogsPath;
            if (Directory.Exists(deployedLogsPath))
                return deployedLogsPath;
            var fallback = Path.Combine(executableDir, "logs");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        internal static void SafeShutdown(IContainer container)
        {
            try { container?.Dispose(); }
            catch
            {
                // ignored
            }

            try { Log.CloseAndFlush(); }
            catch
            {
                // ignored
            }
        }
    }
}