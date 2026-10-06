using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LogModelAttribute : Attribute
    {
        Type Tipo = null;

        /// <summary>
        /// Utilizado para informar qual classe de Log
        /// </summary>
        /// <param name="logModel"></param>
        public LogModelAttribute(Type tipoLog)
        {
            this.Tipo = tipoLog;
        }

        public Type GetTipoLog() 
        {
            return Tipo;
        }
    }
}
