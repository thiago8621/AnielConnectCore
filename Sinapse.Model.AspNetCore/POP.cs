using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Model.AspNetCore
{
    public class POP : BaseModel
    {
        public virtual String Nome { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual Filial Filial { get; set; }
        public virtual String CodCt { get; set; }
        public virtual String Projeto { get; set; }

        #region Endereço

        public virtual String Cep { get; set; }
        public virtual String Endereco { get; set; }
        public virtual String Numero { get; set; }
        public virtual Int64? IdBairro { get; set; }
        public virtual String Bairro { get; set; }
        public virtual Int64? IdCidade { get; set; }
        public virtual String Cidade { get; set; }
        public virtual String UF { get; set; }
        public virtual String Complemento { get; set; }

        public virtual Double? Latitude { get; set; }
        public virtual Double? Longitude { get; set; }

        #endregion
    }
}
