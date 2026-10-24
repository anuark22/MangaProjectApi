using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Models;

namespace MangaProjectApi.CustomMiddleware
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AdminOnlyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            // authorization
            var userInfo = (UsersDTO)context.HttpContext.Items["Client"];
            var adminOnly = context.ActionDescriptor.EndpointMetadata.OfType<AdminOnlyAttribute>().Any();
            if (userInfo == null)
            {
                context.Result = new JsonResult(new
                {
                    errorDescription = "You're not authorized",
                    error = true,
                    Status = 401,
                })
                { StatusCode = StatusCodes.Status401Unauthorized };
            }
            if (adminOnly && userInfo?.RoleId == (int)RolesEnum.Client)
            {
                context.Result = new JsonResult(new
                {
                    errorDescription = "Forbidden",
                    error = true,
                    Status = 403
                })
                { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
