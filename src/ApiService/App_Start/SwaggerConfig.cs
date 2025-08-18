using System.Web;
using System.Web.Http;
using ApiService;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace ApiService;

public class SwaggerConfig
{
    public static void Register()
    {
        GlobalConfiguration.Configuration
            .EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "Cincinnati Unified Safety Toolkit API");

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