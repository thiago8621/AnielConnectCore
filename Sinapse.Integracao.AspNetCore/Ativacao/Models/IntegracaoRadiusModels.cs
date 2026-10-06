using Aniel.Connect.Core.AspNetCore;
using Sinapse.Model.AspNetCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Models
{
    public class PlanoProdutoRadiusRequest
    {
        public PlanoProdutoRadiusRequest() { }
        public PlanoProdutoRadiusRequest(long idPlanoProduto)
        {
            this.IdClientePlanoProduto = idPlanoProduto;
        }

        public long IdClientePlanoProduto { get; set; }
        public string Usuario { get; set; }
    }
    public class ClientesRadiusRequest
    {
        public List<long> ListIdCliente { get; set; }
    }

    public class GetHistoricoConexaoRadius_DataTableRequest : DataTableRequest
    {
        public virtual Int64 Id_Login_Radius { get; set; }
        public virtual string Login_Radius { get; set; }
    }

    public class GetHistoricoConexaoRadius_Response
    {
        public List<RelatorioHistoricoConexaoModel> lista { get; set; }
        public Int32 recordsTotal { get; set; }
    }

    public class LoginRadiusModel
    {
        public virtual long IdLogin { get; set; }
        public virtual long IdPlano { get; set; }
        public virtual long IdClientePlanoProduto { get; set; }
        public virtual string DescricaoPlano { get; set; }
        public virtual long IdCliente { get; set; }
        public virtual string Login { get; set; }
        public virtual string Senha { get; set; }
        public virtual string EnderecoIPV4 { get; set; }
        public virtual string EnderecoIPV6 { get; set; }
        public virtual bool UtilizaQoSProfile { get; set; }
        public virtual string IdQoSProfile { get; set; }
        public virtual string IdQoSProfileReduzido { get; set; }
        public virtual string VelocidadeDownloadPlano { get; set; }
        public virtual string VelocidadeUploadPlano { get; set; }
        public virtual string VelocidadeDownloadReduzidaPlano { get; set; }
        public virtual string VelocidadeUploadReduzidaPlano { get; set; }
        public virtual string Senha_WIFI { get; set; }
        public virtual Concentradora Concentradora { get; set; }
        public virtual bool SituacaoPlanoAtivo { get; set; }
        public virtual bool UtilizaPolicyNameIPv6 { get; set; }
        public virtual bool usaRadiusSemContainer { get; set; }
        public virtual string IdPolicyNameUpload { get; set; }
        public virtual string IdPolicyNameDownload { get; set; }
        public virtual string IdPolicyNameDownloadReduzido { get; set; }
        public virtual string IdPolicyNameUploadReduzido { get; set; }
        public virtual string MAC { get; set; }
        public virtual Pool_IpModel Pool { get; set; }
        public virtual Model.AspNetCore.enumerados.Status_Autenticacao_Radius? Status_Ultima_Autenticacao_Radius { get; set; }
        public virtual DateTime? DataHora_Ultima_Autenticacao_Radius { get; set; }
    }

    public class Pool_IpModel
    {
        public virtual String Descricao { get; set; }
        public virtual Boolean IsPoolBloqueio { get; set; }
        public virtual Boolean IsPoolConcentradora { get; set; }
        public virtual Boolean IsDistribuicaoBlocoIp { get; set; }
        public virtual Boolean IsPoolIpFixo { get; set; }
        public virtual Boolean IsPoolReducaoVelocidade { get; set; }
        public virtual string NomePoolConcentradora { get; set; }
        public virtual long Id_Pool_Ip { get; set; }
        public virtual bool IsPoolIPv4 { get; set; }
    }

    public class DadosConexoesAtivasModel
    {
        public virtual string Login_Radius { get; set; }
        public virtual long Id_Cliente_Plano_Produto { get; set; }
        public virtual string Endereco_IPv4 { get; set; }
        public virtual string Endereco_IPv6 { get; set; }
        public virtual string Endereco_IPv6_Prefix { get; set; }
        public virtual string MAC { get; set; }
        public virtual DateTime? DataHora_Atualizacao { get; set; }
        public virtual string Nome_Pool_Concentradora { get; set; }
        public virtual string Ultimo_Reply { get; set; }
        public virtual DateTime? DataHora_Ultimo_Reply { get; set; }
    }
}