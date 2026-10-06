using Microsoft.AspNetCore.Mvc.Filters;
using Sinapse.DAO.AspNetCore;

namespace Aniel.Connect.Core.AspNetCore.Attributes
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            CONSTANTES_CORE.log4Net.Error(context.Exception);
        }
    }
}
