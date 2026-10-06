using Sinapse.Model.AspNetCore.nhibernate;
using System;

namespace Sinapse.Model.AspNetCore
{
    public class Faixa_Comissionamento_Item : BaseModel
    {
        public virtual Faixa_Comissionamento Faixa_Comissionamento { get; set; }

        public virtual Double? Faixa_Inicial { get; set; }
        public virtual Double? Faixa_Final { get; set; }
        public virtual Double? Coeficiente { get; set; }
    }
}
