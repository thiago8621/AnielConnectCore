using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Model.AspNetCore
{
    public class Radius : BaseModel
    {
        public virtual String Descricao { get; set; }
        public virtual Status_Default? Status { get; set; }

        public virtual String Ip { get; set; }
        public virtual String Porta_SSH { get; set; }
        public virtual Int32 Timeout { get; set; }

        public virtual String Senha { get; set; }
        public virtual String Dominio { get; set; }

        public virtual String GrupoRadius { get; set; }

        #region CAMPOS ANTIGOS (mantidos para compatibilidade temporaria)

        public virtual String Usuario { get; set; }

        public virtual String Banco { get; set; }
        public virtual String Usuario_Banco { get; set; }
        public virtual String Senha_Banco { get; set; }
        public virtual String Porta_Banco { get; set; }

        #endregion

    }
}
