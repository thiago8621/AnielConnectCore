using Microsoft.AspNetCore.Http;

namespace Sinapse.Core.AspNetCore
{
    public static class UserConnectionFactory
    {
        public static IUserConnectionControl GetConnectionControl(HttpContext httpContext, TipoCC config)
        {
            switch (config)
            {
                case TipoCC.Win32: return new UserConnectionControlWin32(httpContext);
                case TipoCC.Web: return new UserConnectionControlWeb(httpContext);
                default: throw new InvalidUserConnectionControl();
            }
        }
    }
}
