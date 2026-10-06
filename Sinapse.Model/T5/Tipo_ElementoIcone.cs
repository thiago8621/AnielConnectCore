using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class Tipo_ElementoIcone : BaseModel
    {
        public virtual String Descricao { get; set; }
        public virtual String Tipo { get; set; }
        public virtual String Url { get; set; }
        public virtual String Arquivo { get; set; }
        public virtual String Caminho { get; set; }
        public virtual Padrao Padrao_Connect { get; set; }
    }
}
