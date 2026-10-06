using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class POP : BaseModel
    {
        public virtual String Nome { get; set; }
        public virtual String Descricao { get; set; }

        public virtual Empresa Empresa { get; set; }
        public virtual Filial Filial { get; set; }
        public virtual String CodCt { get; set; }
        public virtual String Projeto { get; set; }
        public virtual Status_Default Status { get; set; }
      

        #region Endereço

        public virtual String Cep { get; set; }
        public virtual String Endereco { get; set; }
        public virtual String Numero { get; set; }
        public virtual Int64? IdBairro { get; set; }
        public virtual String Bairro { get; set; }
        public virtual Int64? IdCidade { get; set; }
        public virtual long? IdPais { get; set; }
        public virtual String Pais { get; set; }
        public virtual String Cidade { get; set; }
        public virtual String UF { get; set; }
        public virtual String Complemento { get; set; }

        public virtual Double? Latitude { get; set; }
        public virtual Double? Longitude { get; set; }
        public virtual string CodigoAuxiliar { get; set; }

        #endregion
    }
}
