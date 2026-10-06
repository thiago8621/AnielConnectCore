using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Aniel.Connect.Core.AspNetCore.Helpers
{
    public static class HtmlExtensions
    {
        public static IHtmlContent GetStringToast(IHtmlContent sumary)
        {
            String _inicio = "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><ul><li>";
            String _fim = "</li></ul></div>";
            String _meio = "</li><li>";

            var tmp = HttpUtility.HtmlDecode(sumary.ToString())
                                 .Replace(Environment.NewLine, "")
                                 .Replace("'", "\"")
                                 .Replace(_inicio, String.Empty)
                                 .Replace(_fim, String.Empty);

            var lista = tmp.Split(new string[] { _meio }, StringSplitOptions.None).Select(p => String.Format("'{0}'", p));

            return new StringHtmlContent(String.Format("[{0}]", String.Join(",", lista)));
        }

        public static async Task<string> RenderRazorViewToString(IViewEngine viewEngine, Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);
                var viewContext = 
                    new ViewContext(
                        controller.ControllerContext,
                        viewResult.View,
                        controller.ViewData,
                        controller.TempData,
                        sw,
                        new HtmlHelperOptions()
                    );

                await viewResult.View.RenderAsync(viewContext);
                //viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
