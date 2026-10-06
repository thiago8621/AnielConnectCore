using Sinapse.Model.AspNetCore.nhibernate;
using System;

namespace Sinapse.Model.AspNetCore
{
    public class Departamento : BaseModel
    {
        public virtual String Num_Depto { get; set; }
        public virtual String Cod_Depto { get; set; }
        public virtual String Descricao { get; set; }
    }
}
