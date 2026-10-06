using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Aniel.Connect.Core.AspNetCore.Attributes
{
    public class RequireHttpsAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext authFilterContext)
        {
            if (!authFilterContext.HttpContext.Request.IsHttps)
            {
                authFilterContext.HttpContext.Response.StatusCode = 403;
            }
        }
    }
}
