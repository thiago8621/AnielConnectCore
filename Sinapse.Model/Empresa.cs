using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class Empresa : BaseModel
    {
        public virtual Int16 CodEmpresa { get; set; }
        public virtual String Nome { get; set; }
        public virtual String Razao_Social { get; set; }

        public virtual DateTime Data_Alteracao { get; set; }
        public virtual Byte[] Logotipo { get; set; }

        public override string ToString()
        {
            return Razao_Social;
        }
    }
}
