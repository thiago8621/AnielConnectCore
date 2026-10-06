using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Aniel.Connect.Core.AspNetCore.Swagger
{
    public class HideInDocsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach(var apiDescription in context.ApiDescriptions)
            {

                if (!apiDescription.ActionDescriptor.GetType().GetCustomAttributes(typeof(HideInDocsAttribute), true).Any())
                    continue;

                var route = "/" + apiDescription.RelativePath.TrimEnd('/');
                swaggerDoc.Paths.Remove(route);
            }
        }
    }
}
