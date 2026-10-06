using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;

namespace Sinapse.Model.AspNetCore
{
    public class Banco : BaseModel
    {
        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual Status_Default Status { get; set; }
        public virtual String Carteira { get; set; }

        public virtual Int64? Nosso_Numero_Max{ get; set; }

        public override string ToString()
        {
            return Descricao;
        }
    }
}
