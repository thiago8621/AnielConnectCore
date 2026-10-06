using Sinapse.Model;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    public class ConsultaSerasaDados : BaseModel
    {
        public virtual String consulta { get; set; }
        public virtual String cpf { get; set; }
        public virtual String cnpj { get; set; }
        public virtual IntegracaoExternaConsultaCredEnum TipoIntegracao { get; set; }
        public virtual string ParamLoja { get; set; }
    }
}
