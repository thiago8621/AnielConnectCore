using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Reflection;

namespace Sinapse.Core
{
    public static class ResouceManagerService
    {
        public static String GetDisplay(String pString)
        {
            if (String.IsNullOrEmpty(pString))
                return "";
            else
            {
                try
                {
                    String _i18nStr = ConfigurationManager.AppSettings["I18N_Models"];
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
