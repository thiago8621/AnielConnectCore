using NHibernate;
using Sinapse.Integracao.AspNetCore.Ativacao.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Integracao.AspNetCore.Ativacao.Interfaces
{
    public interface IntegracaoConcentradora
    {
        Task ConfiguracaoPlanoCliente(ISession session, LoginRadiusModel login);
        Task ReduzirVelocidade(ISession session, ISession sessionConnect, LoginRadiusModel login);
        Task NormalizarVelocidade(ISession session, ISession sessionConnect, LoginRadiusModel login);

        /// <summary>
        ///     Envia um pacote CoA (Change of Authorization) para a concentradora, que permite alterar a 
        ///     velocidade da internet do cliente sem precisar o desconectar.
        /// </summary>
        void AplicarModificacaoVelocidade(Sinapse.Model.AspNetCore.Radius servidor, string usuario, string ipConcentradora, string velocidade, bool usaRadiusSemContainer);

        Task DesbloqueioConfianca(ISession session, LoginRadiusModel planoProduto, String usuario);
        Task EditarPlanoCliente(ISession session, LoginRadiusModel plano);
        Task DeletarPlano(ISession session, long IdClientePlanoProduto);

        Task ConfigurarPoolCliente(ISession session, LoginRadiusModel login);
        Task LimparPoolCliente(ISession session, string usuario, long idCliente);
    }
}
