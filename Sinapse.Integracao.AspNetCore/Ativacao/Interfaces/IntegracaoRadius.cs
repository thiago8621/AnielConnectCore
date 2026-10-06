using NetTools;
using NHibernate;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using Sinapse.Integracao.AspNetCore.Ativacao.Models.enumerados;
using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Interfaces
{
    public interface IntegracaoRadius
    {
        ISession GetSessionRadius();
        #region Concentradora

        Task CadastrarConcentradora(ISession session, Concentradora concentradora);
        Task EditarConcentradora(ISession session, Concentradora concentradora);
        Task DeletarConcentradora(ISession session, long idConcentradora);

        #endregion

        #region Usuário

        Task CadastrarUsuario(ISession session, LoginRadiusModel login);
        Task CadastrarUsuario(ISession session, string usuario, string senha, long idCliente);
        Task FinalizarCadastroUsuario(ISession session, LoginRadiusModel login);

        Task EditarUsuario(ISession session, LoginRadiusModel login);
        Task EditarUsuario(ISession session, string usuario, string senha, long idCliente);
        Task DeletarUsuario(ISession session, long idCliente);

        Task BloquearUsuario(ISession session, string usuario, string ipConcentradora, string portaIncoming, string secret, bool usaRadiusSemContainer);
        Task DesbloquearUsuario(ISession session, string Usuario, string Plano);

        Task BloquearSinalPorPool(ISession session, string usuario, long idCliente, string ipConcentradora, string portaIncoming, string nomePoolBloqueio, bool usaRadiusSemContainer);
        Task DesbloquearSinalPorPool(ISession session, LoginRadiusModel login);

        Task BloquearSinalPorDomain(ISession session, string Login, string IpConcentradora, string PortaIncoming, string IdDomainName, long IdCliente, bool usaRadiusSemContainer);
        Task DesbloquearSinalPorDomain(ISession session, ISession sessionConnect, LoginRadiusModel login, bool usaRadiusSemContainer);

        Task AssociaUsuarioAoPlano(ISession session, string UsuarioPPPOE, string NomePlano, string Prioridade, long IdClientePlanoProduto);

        Task CadastrarPoolIp(ISession session, string PoolName, IPAddressRange RangeIP);
        Task EditarPoolIp(ISession session, string PoolName, IPAddressRange Novo_RangeIP, string Novo_PoolName = null);
        Task DeletarPoolIp(ISession session, string PoolName);

        /// <summary>
        ///     Atualiza o nome do Pool de IP do usuário (Usado para normalização e redução de velocidade).
        /// </summary>
        Task AtualizarPoolIpUsuario(ISession session, long Id_Cliente, string Novo_PoolName);

        /// <summary>
        ///     Atualiza todas as infomações relacionadas ao Pool de IP do usuário (Nome do Pool baseado no tipo dela, IPv4 e IPv6).
        /// </summary>
        Task AtualizarPoolIpUsuario(ISession session, LoginRadiusModel login);

        Task<string> GetPoolBloqueioDisponivel(ISession session, List<string> Pools_Bloqueio);
        Task<string> GetPoolReducaoVelocidadeDisponivel(ISession session, List<String> Pools_Reducao);

        bool TesteLogin(string ipConcentradora, string usuario, string senha, bool usaRadiusSemContainer);

        #endregion

        void ReiniciarServicoRadius();

        Task<GetHistoricoConexaoRadius_Response> GetHistoricoConexao(ISession session, GetHistoricoConexaoRadius_DataTableRequest request);
        Task<List<ConsumoPeriodoModel>> GetConsumoPeriodo(ISession session, String Login_Radius, HistoricoConsumoPeriodoEnum Periodo_Consumo, DateTime? Data_Inicio, DateTime? Data_Fim);
        Task<ConsumoPeriodoModel> GetConexaoTempoReal(ISession session, String Login_Radius, Sinapse.Model.AspNetCore.enumerados.Medida_Velocidade_ConexaoEnum Unidade_Medida_Velocidade);
        Task<List<TempoOnlineOfflineModel>> GetTempoOnline(ISession session, String Login_Radius, DateTime Data_Inicio, DateTime Data_Fim);
        Task<List<TempoOnlineOfflineModel>> GetTempoOffline(ISession session, String Login_Radius, DateTime Data_Inicio, DateTime Data_Fim);

        Task<List<DadosConexoesAtivasModel>> GetDadosConexoesAtivas(ISession session, DateTime? Data_Hora_Corte);

        bool DesconectarLogin(LoginRadiusModel login, bool UsaRadiusSemContainer, bool waitResponse = false);
    }
}
