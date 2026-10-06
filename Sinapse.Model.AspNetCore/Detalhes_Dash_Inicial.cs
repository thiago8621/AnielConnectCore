using Sinapse.Model.AspNetCore.nhibernate;
using System;

namespace Sinapse.Model.AspNetCore
{
    public class Detalhes_Dash_Inicial : BaseModel
    {
        public virtual Perfil_DashBoard_Connect Perfil_DashBoard_Connect { get; set; }
        public virtual Int32? Recurso { get; set; }
    }
}
