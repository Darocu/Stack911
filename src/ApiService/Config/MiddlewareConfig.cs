// src/ApiService/Config/MiddlewareConfig.cs
using Owin;
using Serilog;
using System;
using System.Diagnostics;
using ApiService.Middleware;
using ApiService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Cors;

namespace ApiService.Config
{
    public static class MiddlewareConfig
    {
        internal static void Configure(IAppBuilder app, ILogger logger, IConfigurationRoot configuration)
        {
            // Error handling middleware
            app.Use(async (ctx, next) =>
            {
                try { await next(); }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unhandled exception");
                    ctx.Response.StatusCode = 500;
                }
            });

            // Logging middleware
            app.Use(async (context, next) =>
            {
                var sw = Stopwatch.StartNew();
                logger.Information("Request received: {Method} {Path}", context.Request.Method, context.Request.Path);
                await next.Invoke();
                sw.Stop();
                logger.Information("Request processed in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
            });
            
            // TODO: Configure CORS Middleware
            app.UseCors(CorsOptions.AllowAll);

            app.UseHomePage();
            var apiKeyStore = new ApiKeyStore(configuration);
            app.Use<ApiKeyMiddleware>(apiKeyStore);
        }
    }
}