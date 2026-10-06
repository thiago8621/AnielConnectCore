using System;
using System.Globalization;
using System.Resources;

namespace Sinapse.Core.AspNetCore
{
    public static class I18N_ModelsService
    {        

        public static String GetMens(ResourceManager pResource, String pKeyResouce, params Object[] pArgs)
        {
            CultureInfo culture = new CultureInfo("pt");

            if (pResource == null)
            {
                return Convert.ToString(pKeyResouce);
            }
            else if (pResource.GetString(pKeyResouce.ToUpper()) == null)
            {
                return Convert.ToString(pKeyResouce);
            }
            else
            {
                return String.Format(pResource.GetString(pKeyResouce.ToUpper()), pArgs);
            }
        }

        public static String GetMens(ResourceManager pResource, String pDominio, String pProperty, params Object[] pArgs)
        {
            CultureInfo culture = new CultureInfo("pt");
            String _resource = String.Empty;

            if (pResource == null)
            {
                return pProperty.ToUpper();
            }

            _resource = pResource.GetString(String.Format("{0}_{1}", pDominio.ToUpper(), pProperty.ToUpper()));

            if (String.IsNullOrEmpty(_resource))
            {
                _resource = pResource.GetString(pProperty.ToUpper());
            }

            if (String.IsNullOrEmpty(_resource))
            {                
                return pProperty.ToUpper();
            }
            else
            {
                return String.Format(_resource, pArgs);
            }
        }
    }
}
