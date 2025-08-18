using System.Web.Http;
using System.Web.Http.Cors;

namespace ApiService;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Map all Web API routes
        config.MapHttpAttributeRoutes();
            
        var cors = new EnableCorsAttribute(
            "https://saver-cpd-dev.coc.ads, http://saver-cpd-dev.coc.ads, http://saver-cpd.coc.ads, https://saver-cpd.coc.ads, http://ecc-intranet, https://ecc-intranet, http://localhost:5274", 
            "*",
            "*");
        config.EnableCors(cors);
    }
}