using Sinapse.Core;
using Sinapse.ModelLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{

    [Serializable]
    [ObjetoAuxiliarLog(ForeingKey = "PerfilAcesso.Id", TipoPai = typeof(PerfilAcesso), TipoLog = typeof(PerfilAcessoLog),
                       OBSMessageResourceType = typeof(Resources.Resources),
                       OBSMessageResourceName = "LOGOBS_ACESSOPERFIL")]
    public class AcessosPerfil : BaseModel, IAcessoPerfil
    {
        private List<PaginaSistemaRecurso> _recursos = null;

        public virtual PerfilAcesso PerfilAcesso { get; set; }

        public virtual PaginaSistema PaginaSistema { get; set; }

        public virtual List<PaginaSistemaRecurso> Recursos
        {
            get
            {
                if (_recursos == null)
                    _recursos = new List<PaginaSistemaRecurso>();

                return _recursos;
            }

            set
            {
                _recursos = value;
            }
        }

        public virtual String Controle { get; set; }

        public virtual Int64 IdPagina { get; set; }

        public virtual Boolean TodosRecursos { get; set; }

        public virtual String RecursosJSON { get; set; }

        public virtual String CamposBloqueadosJSON { get; set; }

        public virtual SistemaWeb Sistema { get; set; }

        public override string ToString()
        {
            return String.Concat("Recursos ", RecursosJSON, TodosRecursos? " Todos Recursos" : String.Empty);
        }
    }
}
