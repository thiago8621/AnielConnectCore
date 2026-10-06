using Sinapse.Model.nhibernate;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class Funcionario : BaseModel
    {
        public virtual Int64  Id { get; set; }
        public virtual String Num_Fre { get; set; }
        public virtual String Nome { get; set; }
        public virtual String Tel_Celular { get; set; }
        public virtual Int16? Situacao { get; set; }

        public virtual Boolean Vendedor { get; set; }
        public virtual Funcionario Supervisor { get; set; }

        public virtual Int64? Id_PontoVenda { get; set; }

        public virtual Boolean E_SUPERVISOR { get; set; }

        ////TAMANHO MAXIMO 50 CARACTERES PONTO VENDA
        //public virtual String Codigo { get; set; }

        public virtual Meta Meta { get; set; }
        public virtual Faixa_Comissionamento Faixa { get; set; }
        public virtual Departamento Departamento { get; set; }        
        public virtual long? IdEquipeVenda { get; set; }       
        public virtual ParametrosConsultaPreQual ParametrosConsultaPreQual { get; set; }       
        public virtual Int64? Id_Comissionamento { get; set; }
        public virtual string CodigoVendedorIntegracao { get; set; }

        public virtual Filial Filial { get; set; }

        public virtual String Cpf_Cnpj { get; set; }
        public virtual Int64? Id_CentroCusto { get; set; }
        public virtual String Rg { get; set; }
        public virtual String Endereco { get; set; }
        public virtual String Complemento { get; set; }
        public virtual Int32? Numero { get; set; }
        public virtual String Bairro { get; set; }
        public virtual String Municipio { get; set; }
        public virtual String Cep { get; set; }
        public virtual String Uf { get; set; }
        public virtual String Pais { get; set; }
        public virtual string CodigoVendedorAuxiliar { get; set; }

        public override string ToString()
        {
            return Nome;
        }

    }
}
