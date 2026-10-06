using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace Sinapse.Core.AspNetCore
{
    public static class ResouceManagerService
    {
        public static IConfiguration Configuration;
        public static String GetDisplay(String pString)
        {
            if (String.IsNullOrEmpty(pString))
            {
                return "";
            }
            else
            {
                try
                {
                    if (Configuration == null)
                    {
                        throw new Exception("As configurações da aplicação não foram informadas");
                    }

                    String _i18nStr = Configuration["I18N_Models"];
                    Type i18n_Models = Type.GetType(_i18nStr);

                    MethodInfo getMens = i18n_Models.GetMethod("GetMens", new Type[] { typeof(String) });
                    
                    return (String)getMens.Invoke(i18n_Models, new object[] { pString });
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao carregar o ResModels", ex);
                }
            }
        }
    }
}
