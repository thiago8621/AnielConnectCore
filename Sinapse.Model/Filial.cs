using Sinapse.Model.enumerado;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model
{
    public class Filial : BaseModel
    {
        public virtual Int16 CodFilial { get; set; }
        public virtual String Nome { get; set; }

        public virtual String Cnpj { get; set; }

        public virtual Empresa Empresa { get; set; }

        public virtual Comissionamento Comissionamento { get; set; }
        public virtual Faixa_Comissionamento Faixa_Comissionamento { get; set; }
        public virtual Meta Meta { get; set; }

        public virtual String Endereco { get; set; }
        public virtual String Complemento { get; set; }
        public virtual Int32? Numero { get; set; }
        public virtual String Bairro { get; set; }
        public virtual String Cidade { get; set; }
        public virtual String Cep { get; set; }
        public virtual String Uf { get; set; }

        public virtual String Telefone { get; set; }

        public virtual string CodigoCedente { get; set; }
        public virtual string DigitoVerificadorCedente { get; set; }
        public virtual string Carteira { get; set; }

        public virtual Natureza_TributacaoEnum Natureza_tributacao { get; set; }
        public virtual string Razao_Social { get; set; }
        public virtual Boolean Optante_Simples_Nacional { get; set; }
        public virtual Regime_TributarioEnum Regime_Tributario { get; set; }
        public virtual Boolean Incentivo_Cultural { get; set; }
        public virtual Boolean Incentivo_Fiscal { get; set; }
        public virtual Regime_Tributario_EspecialEnum Regime_Tributario_Especial { get; set; }
        public virtual string DDD { get; set; }
        public virtual string Email { get; set; }
        public virtual string CNAE_Fiscal { get; set; }
        public virtual Tipo_Tributacao_ISS Tipo_Tributacao_ISS { get; set; }
        public virtual Exigibilidade_ISS Exigibilidade_ISS { get; set; }
        public virtual string Num_Processo_Suspensao { get; set; }

        public virtual string Inscricao_Municipal { get; set; }
        public virtual string Inscricao_Estadual { get; set; }

        public virtual Int64 Proximo_Sequencial_Nfse { get; set; }
        public override string ToString()
        {
            return Nome;
        }

    }
}
