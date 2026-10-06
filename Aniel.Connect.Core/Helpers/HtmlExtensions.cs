using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Aniel.Connect.Core.Helpers
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString GetStringToast(MvcHtmlString sumary)
        {
            String _inicio = "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><ul><li>";
            String _fim = "</li></ul></div>";
            String _meio = "</li><li>";

            var tmp = HttpUtility.HtmlDecode(sumary.ToHtmlString()
                                 .Replace(System.Environment.NewLine, ""))
                                 .Replace("'", "\"")
                                 .Replace(_inicio, String.Empty)
                                 .Replace(_fim, String.Empty);

            var lista = tmp.Split(new string[] { _meio }, StringSplitOptions.None).Select(p => String.Format("'{0}'", p));

            return MvcHtmlString.Create(String.Format("[{0}]", String.Join(",", lista)));
        }

        public static string RenderRazorViewToString(ControllerBase controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
