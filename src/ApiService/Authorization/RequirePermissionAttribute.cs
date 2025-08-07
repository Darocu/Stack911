using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ApiService.Authorization;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    protected override bool IsAuthorized(HttpActionContext actionContext)
    {
        var user = actionContext.ControllerContext.RequestContext.Principal as ClaimsPrincipal;
        if (user == null) return false;

        // Allow if user has "Administrator" or any required permission
        return user.Claims.Any(c =>
            c.Type == "Permission" &&
            (_permissions.Contains(c.Value) || c.Value == "Administrator"));
    }

    protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
    {
        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Insufficient permissions.");
    }
}