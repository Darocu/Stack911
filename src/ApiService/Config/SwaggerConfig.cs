using System.Web.Http;
using Swashbuckle.Application;

namespace ApiService.Config;

public class SwaggerConfig
{
    internal static void Register(HttpConfiguration config)
    {
        config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "Cincinnati Unified Safety Toolkit");

                // Optional: Include XML comments for better docs
                // var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + @"\bin\YourApi.xml";
                // c.IncludeXmlComments(xmlPath);
                
                c.ApiKey("apiKey")
                    .Description("API Key Authentication")
                    .Name("X-API-Key")
                    .In("header");
            })
            .EnableSwaggerUi(c =>
            {
                c.EnableApiKeySupport("X-API-Key", "header");
            });
    }
}