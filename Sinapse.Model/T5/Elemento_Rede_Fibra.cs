using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class Elemento_Rede_Fibra : BaseModel
    {
        public virtual Elemento_Rede Elemento_Cabo { get; set; }
        public virtual long numLoose { get; set; }
        public virtual long numFibra { get; set; }
        public virtual Int64? Cliente_Plano_Prod { get; set; }
        public virtual long? Ponta_A { get; set; }
        public virtual long? Conexao_Ponta_A { get; set; }
        public virtual long? Ponta_B { get; set; }
        public virtual long? Conexao_Ponta_B { get; set; }
    }
}
