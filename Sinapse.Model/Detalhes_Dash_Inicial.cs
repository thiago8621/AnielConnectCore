using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class Detalhes_Dash_Inicial : BaseModel
    {
        public virtual Perfil_DashBoard_Connect Perfil_DashBoard_Connect { get; set; }
        public virtual Int32? Recurso { get; set; }
    }
}
