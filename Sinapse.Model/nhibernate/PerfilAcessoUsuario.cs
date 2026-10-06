using Sinapse.Model.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.nhibernate
{

    [Serializable]
    public class PerfilAcessoUsuario : BaseModel
    {
        public virtual PerfilAcesso PerfilAcesso { get; set; }

        public virtual IdentityUser Usuario { get; set; }
    }
}
