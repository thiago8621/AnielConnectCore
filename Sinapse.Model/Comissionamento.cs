using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class Comissionamento : BaseModel
    {
        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual Int64 Id_Filial { get; set; }
        public virtual Status_Default? Status { get; set; }
    }
}
