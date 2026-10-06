using System;
using Sinapse.Model.nhibernate;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sinapse.Model.enumerados;

namespace Sinapse.Model
{
    public class Faixa_Comissionamento_Item : BaseModel
    {
        public virtual Faixa_Comissionamento Faixa_Comissionamento { get; set; }

        public virtual Double? Faixa_Inicial { get; set; }
        public virtual Double? Faixa_Final { get; set; }
        public virtual Double? Coeficiente { get; set; }
    }
}
