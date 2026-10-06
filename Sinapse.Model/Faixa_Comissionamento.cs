using System;
using Sinapse.Model.nhibernate;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sinapse.Model.enumerados;

namespace Sinapse.Model
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
