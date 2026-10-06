using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    /// <summary>
    ///     TODO: Estudar se é possivel passar o mapeamento para o projeto do Connect
    ///           para poder utilizar os enumerados nos campos TipoCriterio e TipoOperador
    /// </summary>
    public class CriteriosParamCredito : BaseModel
    {
        public virtual string Criterio { get; set; }
        public virtual string Operador { get; set; }
        public virtual int TipoCriterio { get; set; }
        public virtual int TipoOperador { get; set; }
        public virtual Int64 Valor { get; set; }
        public virtual Int64 Pontos { get; set; }
        public virtual Boolean Eliminatorio { get; set; }

        public virtual ParametrosConsultaCredito Id_ParametrosConsultaCredito { get; set; }

    }
}
