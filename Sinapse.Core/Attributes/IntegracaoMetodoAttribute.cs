using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Core.Attributes
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
