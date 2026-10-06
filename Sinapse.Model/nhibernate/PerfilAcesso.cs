using Sinapse.ModelLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{
    [Serializable]
    [ObjetoPrincipalLog(TypeLog = typeof(PerfilAcessoLog),
                        OBSMessageResourceType = typeof(Resources.Resources),
                        OBSMessageResourceName = "LOGOBS_PERFILACESSO")]
    public class PerfilAcesso : BaseModel, IPerfilAcesso
    {
        public virtual Int32 Codigo { get; set; }

        public virtual String Descricao { get; set; }

        public virtual IEnumerable<PerfilAcessoUsuario> PerfilUsuario { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Descricao))
                return String.Empty;
            else
                return Descricao;
        }

    }
}
