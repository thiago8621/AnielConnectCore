using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class ModeloMensagem : BaseModel
    {
        public virtual String Nome { get; set; }
        public virtual String Texto { get; set; }
        public virtual String IdWhatsAppBusinessAccount { get; set; }
        public virtual String Status { get; set; }
    }
}
