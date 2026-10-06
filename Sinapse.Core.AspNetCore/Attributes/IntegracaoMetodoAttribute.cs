using System;

namespace Sinapse.Core.AspNetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IntegracaoMetodoAttribute : Attribute
    {
        public String NomeObjeto { get; set; }

        public Type TipoParametro { get; set; }


        private IntegracaoMetodoAttribute()
        {

        }

        public IntegracaoMetodoAttribute(String pNomeObjeto, Type pTipoParametro)
        {
            NomeObjeto = pNomeObjeto;
            TipoParametro = pTipoParametro;
        }
    }
}
