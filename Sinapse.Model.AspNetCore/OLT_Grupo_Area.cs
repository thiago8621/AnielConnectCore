using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Model.AspNetCore
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
