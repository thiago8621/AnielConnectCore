using System;

namespace Sinapse.Model.AspNetCore
{
    public class UsuarioT5
    {
        public virtual String Usuario { get; set; }
        public virtual String Nome { get; set; }
        public virtual String Administrador { get; set; }

        public virtual String Senha { get; set; }
        public virtual String Ativo { get; set; }

        public virtual Funcionario Funcionario { get; set; }

        public virtual Int64? Perfil_Dash_Cherry { get; set; }
        public virtual String Desc_Perfil_Dash_Cherry { get; set; }
    }
}
