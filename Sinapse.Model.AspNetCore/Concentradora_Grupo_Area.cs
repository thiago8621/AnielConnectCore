using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Model.AspNetCore
{
    public class Concentradora_Grupo_Area : BaseModel
    {
        public virtual String Cod_Escritorio { get; set; }
        public virtual String Cod_Area { get; set; }
        public virtual int? Cod_Localidade { get; set; }
        public virtual String Desc_Escritorio { get; set; }
        public virtual String Desc_Area { get; set; }
        public virtual String Localidade { get; set; }
        public virtual Concentradora Concentradora { get; set; }
        public virtual Status_Default Status { get; set; }
    }
}
