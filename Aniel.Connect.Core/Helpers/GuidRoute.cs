using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Aniel.Connect.Core
{
    public class GuidRoute : Route
    {
        private readonly bool _isGuidRoute;

        public GuidRoute(string uri, object defaults, String[] namespaces)
            : base(uri, new RouteValueDictionary(defaults), new MvcRouteHandler())
        {
            _isGuidRoute = uri.Contains("guid");

            var dataTokens = new RouteValueDictionary {
                                {
                                    "Namespaces", namespaces
                                }
                            };
            
            DataTokens = dataTokens;

        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext);
            if (routeData == null)
                return null;
            
            if (!routeData.Values.ContainsKey("guid") ||
            routeData.Values["guid"].ToString() == "")
                routeData.Values["guid"] = Guid.NewGuid().ToString("N");
            

            return routeData;
        }

        public override VirtualPathData GetVirtualPath
        (RequestContext requestContext, RouteValueDictionary values)
        {
            return !_isGuidRoute ? null : base.GetVirtualPath(requestContext, values);
        }
    }
}