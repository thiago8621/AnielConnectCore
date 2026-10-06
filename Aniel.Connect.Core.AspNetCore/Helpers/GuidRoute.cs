using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aniel.Connect.Core.AspNetCore
{
    // O sistema de rotas do ASP.NET Core utiliza uma classe diferente da utilizada na versão antiga do .NET,
    // não encontrei uma forma de reproduzir o comportamento no ASP.NET Core

    //public class GuidRoute : Route
    //{
    //    private readonly bool _isGuidRoute;

    //    public GuidRoute(string uri, object defaults, String[] namespaces)
    //        : base(uri, new RouteValueDictionary(defaults), new MvcRouteHandler())
    //    {
    //        _isGuidRoute = uri.Contains("guid");

    //        var dataTokens = new RouteValueDictionary {
    //                            {
    //                                "Namespaces", namespaces
    //                            }
    //                        };
            
    //        DataTokens = dataTokens;

    //    }

    //    public override RouteData GetRouteData(HttpContextBase httpContext)
    //    {
    //        var routeData = base.GetRouteData(httpContext);
    //        if (routeData == null)
    //            return null;
            
    //        if (!routeData.Values.ContainsKey("guid") ||
    //        routeData.Values["guid"].ToString() == "")
    //            routeData.Values["guid"] = Guid.NewGuid().ToString("N");
            

    //        return routeData;
    //    }

    //    public override VirtualPathData GetVirtualPath
    //    (RequestContext requestContext, RouteValueDictionary values)
    //    {
    //        return !_isGuidRoute ? null : base.GetVirtualPath(requestContext, values);
    //    }
    //}
}