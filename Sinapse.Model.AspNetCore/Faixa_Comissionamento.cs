using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model.AspNetCore
{
    public class Faixa_Comissionamento : BaseModel
    {
        IList<Faixa_Comissionamento_Item> _itens;

        public virtual Int64 Codigo { get; set; }
        public virtual String Descricao { get; set; }

        public virtual IList<Faixa_Comissionamento_Item> Itens
        {
            get
            {
                if (_itens == null)
                    _itens = new List<Faixa_Comissionamento_Item>();
                return _itens;
            }
            set
            {
                _itens = value;
            }
        }

        public virtual Status_Default? Status { get; set; }
    }
}
