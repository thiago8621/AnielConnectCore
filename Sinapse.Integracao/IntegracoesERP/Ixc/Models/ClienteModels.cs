using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.IntegracoesERP.Ixc.Models
{
    public class ClienteResponse
    {
        public string id { get; set; }
        public string razao { get; set; }
        public string fantasia { get; set; }
        public string tipo_assinante { get; set; }
        public string cpf_pai { get; set; }
        public string cpf_mae { get; set; }

        public string id_tipo_cliente { get; set; }
        public string tipo_pessoa { get; set; }
        public string cnpj_cpf { get; set; }
        public string ie_identidade { get; set; }

        public string rg_orgao_emissor { get; set; }
        public string inscricao_municipal { get; set; }
        public string email { get; set; }
        public string ativo { get; set; }

        public string endereco { get; set; }
        public string numero { get; set; }
        public string complemento { get; set; }
        public string bairro { get; set; }

        public string cidade { get; set; }
        public string uf { get; set; }
        public string cep { get; set; }
        public string telefone_celular { get; set; }

        public string telefone_comercial { get; set; }
        public string fone { get; set; }
        public string apartamento { get; set; }
        public string bloco { get; set; }
        public string referencia { get; set; }
        //
        public string latitude { get; set; }
        public string longitude { get; set; }
        //
        public string senha { get; set; }
        //
        public string nome_pai { get; set; }
        public string nome_mae { get; set; }
        //
        public string data_nascimento { get; set; }
        //       
        public string nacionalidade { get; set; }
        public string estado_civil { get; set; }
        public string sexo { get; set; }
        //cobranca
        public string cep_cob { get; set; }
        public string endereco_cob { get; set; }
        public string numero_cob { get; set; }
        public string bairro_cob { get; set; }
        public string cidade_cob { get; set; }
        public string complemento_cob { get; set; }
        public string uf_cob { get; set; }

    }

    public class TipoClienteResponse
    {
        public string id { get; set; }
        public string tipo_cliente { get; set; }
        public string tipo_cliente_scm { get; set; }
        public string contribuinte_icms { get; set; }
        public string tipo_assinante { get; set; }
    }
}