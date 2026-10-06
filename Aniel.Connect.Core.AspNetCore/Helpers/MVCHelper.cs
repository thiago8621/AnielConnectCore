using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Aniel.Connect.AspNetCore.Core
{
    public static class MVCHelpers
    {
        // Render BootStrap menu item with active class
        public static IHtmlContent MenuItem(
            this HtmlHelper htmlHelper,
            string text,
            string action,
            string controller,
            object routeValues = null,
            object htmlAttributes = null
        )
        {
            var li = new TagBuilder("li");
            
            var routeData = htmlHelper.ViewContext.RouteData;

            string currentAction = (string)routeData.Values["action"];
            string currentController = (string)routeData.Values["controller"];

            if (string.Equals(currentAction,
                              action,
                              StringComparison.OrdinalIgnoreCase) &&
                string.Equals(currentController,
                              controller,
                              StringComparison.OrdinalIgnoreCase))
            {
                li.AddCssClass("active");
            }
            if (routeValues != null)
            {
                if (htmlAttributes != null)
                {
                    li.InnerHtml.AppendHtml(
                        htmlHelper.ActionLink(
                            text,
                            action,
                            controller,
                            routeValues,
                            htmlAttributes
                        )
                    );
                }
                else
                {
                    li.InnerHtml.AppendHtml(
                        htmlHelper.ActionLink(
                            text,
                            action,
                            controller,
                            routeValues
                        )
                    );
                }
            }
            else
            {
                if (htmlAttributes != null)
                {
                    li.InnerHtml.AppendHtml(
                        htmlHelper.ActionLink(
                            text,
                            action,
                            controller,
                            null,
                            htmlAttributes
                        )
                    );
                }
                else
                {
                    li.InnerHtml.AppendHtml(
                        htmlHelper.ActionLink(
                            text,
                            action,
                            controller
                        )
                    );
                }
            }
            return li.RenderBody();
        }

        // As the text the: "<span class='glyphicon glyphicon-plus'></span>" can be entered
        public static IHtmlContent NoEncodeActionLink(
            this HtmlHelper htmlHelper,
            string text, 
            string title, 
            UrlActionContext actionContext,
            object htmlAttributes = null
        )
        {
            UrlHelper urlHelper = new UrlHelper(htmlHelper.ViewContext);

            TagBuilder builder = new TagBuilder("a");
            builder.InnerHtml.Append(text);
            builder.Attributes["title"] = title;
            builder.Attributes["href"] = urlHelper.Action(actionContext);
            builder.MergeAttributes(new RouteValueDictionary(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)));

            return builder.RenderBody();
        }

        public static IHtmlContent JSonObjectToTable(this HtmlHelper htmlHelper, String json)
        {
            StringBuilder table = new StringBuilder();

            try
            {
                dynamic objetoJSon = JsonConvert.DeserializeObject(json);
            
                table.Append("<table class='table table-striped table-bordered datatable'> <thead><tr>");

                foreach (Newtonsoft.Json.Linq.JProperty prop in objetoJSon)
                {
                    table.Append("<th>" + prop.Name + "</th>");
                }

                table.Append("</tr></thead><tbody id='dataTablesbody'><tr>");

                foreach (Newtonsoft.Json.Linq.JProperty prop in objetoJSon)
                {
                    table.Append("<td>" + Convert.ToString(prop.Value) + "</td>");
                }

                table.Append("</tr></tbody></table>");

                return new StringHtmlContent(table.ToString());
            }
            catch
            {
                return new StringHtmlContent(json);
            }
        }

        
    }
}