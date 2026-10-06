using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class Departamento : BaseModel
    {
        public virtual String Num_Depto { get; set; }
        public virtual String Cod_Depto { get; set; }
        public virtual String Descricao { get; set; }
    }
}
