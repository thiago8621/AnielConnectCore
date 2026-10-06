using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public class DisplayNameAttribute : Attribute
    {
        String Key_Resource = "";

        /// <summary>
        /// Informe o Key do Resource ResModels
        /// </summary>
        /// <param name="key_Resource"></param>
        public DisplayNameAttribute(String key_Resource)
        {
            this.Key_Resource = key_Resource;
        }

        public String GetDisplayName() 
        {
            return Key_Resource;
        }
    }
}
