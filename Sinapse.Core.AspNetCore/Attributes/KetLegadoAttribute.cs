using System;

namespace Sinapse.Core.AspNetCore
{
    public class KetLegadoAttribute : Attribute
    {
        String[] Keys;

        /// <summary>
        /// Informe o Key do Resource ResModels
        /// </summary>
        /// <param name="key_Resource"></param>
        public KetLegadoAttribute(String[] pKeys)
        {
            this.Keys = pKeys;
        }

        public String[] GetKeys() 
        {
            return Keys;
        }
    }
}
