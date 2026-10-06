using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class SMS : BaseModel
    {
        public virtual string Mensagem { get; set; }
        public virtual bool Enviado { get; set; }
        public virtual string Num_Celular { get; set; }
        public virtual Tipo_Integracao_SMS Tipo_Integracao { get; set; }
    }
}
