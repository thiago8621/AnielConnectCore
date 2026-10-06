using System;

namespace Sinapse.Core.AspNetCore
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
