using Microsoft.AspNetCore.Html;
using System;
using System.Linq;
using System.Net;

namespace Aniel.Connect.Core.Helpers
{
    public static class HtmlExtensionsCore
    {
        public static IHtmlContent GetStringToast(IHtmlContent sumary)
        {
            String _inicio = "<div class=\"validation-summary-errors\" data-valmsg-summary=\"true\"><ul><li>";
            String _fim = "</li></ul></div>";
            String _meio = "</li><li>";

            var tmp = WebUtility.HtmlDecode(sumary.ToString())
                                 .Replace(Environment.NewLine, "")
                                 .Replace("'", "\"")
                                 .Replace(_inicio, String.Empty)
                                 .Replace(_fim, String.Empty);

            var lista = tmp.Split(new string[] { _meio }, StringSplitOptions.None).Select(p => String.Format("'{0}'", p));

            return new HtmlString(String.Format("[{0}]", String.Join(",", lista)));
        }
    }
}
