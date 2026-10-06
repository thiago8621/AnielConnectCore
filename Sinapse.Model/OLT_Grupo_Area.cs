using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class OLT_Grupo_Area : BaseModel
    {
        public virtual string Cod_Escritorio { get; set; }
        public virtual string Cod_Area { get; set; }
        public virtual String Desc_Escritorio { get; set; }
        public virtual String Desc_Area { get; set; }
        public virtual OLT OLT { get; set; }
        public virtual Status_Default Status { get; set; }
    }
}
