using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;

namespace Sinapse.Model.AspNetCore
{
    public class Comissionamento : BaseModel
    {
        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual Int64 Id_Filial { get; set; }
        public virtual Status_Default? Status { get; set; }
    }
}
