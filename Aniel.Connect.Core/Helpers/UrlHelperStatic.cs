using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AnielLogistique.Core
{
    public static class UrlHelperStatic
    {
        private static System.Web.Mvc.UrlHelper _urlHelper;

        public static System.Web.Mvc.UrlHelper GetFromContext()
        {
            if (_urlHelper == null)
            {
                if (HttpContext.Current == null)
                {
                    throw new HttpException("Current httpcontext is null!");
                }

                if (!(HttpContext.Current.CurrentHandler is System.Web.Mvc.MvcHandler))
                {
                    throw new HttpException("Type casting is failed!");
                }

                _urlHelper = new System.Web.Mvc.UrlHelper(((System.Web.Mvc.MvcHandler)HttpContext.Current.CurrentHandler).RequestContext);
            }

            return _urlHelper;
        }
    }
}
