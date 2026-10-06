using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IdLogModelAttribute : Attribute
    {
        String PropFK = String.Empty;

        /// <summary>
        /// Utilizado para informar qual Property Chave se refere o objeto
        /// </summary>
        /// <param name="logModel"></param>
        public IdLogModelAttribute(String pPropFK)
        {
            this.PropFK = pPropFK;
        }

        public String GetPropFK() 
        {
            return PropFK;
        }
    }
}
