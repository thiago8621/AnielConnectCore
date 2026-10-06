using Sinapse.ModelLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinapse.Model.nhibernate
{
    [Serializable]
    [ObjetoAuxiliarLog(ForeingKey = "ID_EMPRESA", TipoPai = typeof(Empresa), TipoLog = typeof(EmpresaLog),
                       OBSMessageResourceType = typeof(Resources.Resources),
                       OBSMessageResourceName = "LOGOBS_EMPRESATELEFONE")]
    public class EmpresaTelefone : BaseModel, ITelefone
    {
        public virtual String DDI { get; set; }

        public virtual String DDD { get; set; }

        public virtual String Numero { get; set; }

        public virtual String Ramal { get; set; }

        public virtual String Operadora { get; set; }

        public virtual Int64 ID_EMPRESA { get; set; }

    }
}
