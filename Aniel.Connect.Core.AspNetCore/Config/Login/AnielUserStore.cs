using Aniel.Connect.Core.AspNetCore.Config.Session;
using Microsoft.AspNetCore.Identity;
using NHibernate;
using Sinapse.DAO.AspNetCore.repository;
using Sinapse.Model.AspNetCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.AspNetCore.Config.Login
{
    /// <summary>
    ///     Implementação do IUserStore utilizando a TB_SENHA do banco do Aniel
    /// </summary>
    public class AnielUserStore : IUserStore<UsuarioT5>
    {
        private ISession _session;

        public AnielUserStore(SessionResolverFunc sessionResolver)
        {
            _session = sessionResolver(SessionType.T5);
        }

        // O Point utiliza a mesma tabela de usuários do Aniel, portanto não será necessário criar usuários
        public Task<IdentityResult> CreateAsync(UsuarioT5 user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // O Point utiliza a mesma tabela de usuários do Aniel, portanto não será necessário remover usuários
        public Task<IdentityResult> DeleteAsync(UsuarioT5 user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        // Com o .NET Framework costumávamos a utilizar o User.Identity.GetUserId() para pegar o ID do usuário.
        // Como a primary key da TB_SENHA é o campo USUARIO, que contém o nome de login do usuário, trabalhamos
        // com o nome do usuário quando é feita a busca pelo ID
        public Task<UsuarioT5> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("userId");
            }

            using (var repository = new Conexao<UsuarioT5>(_session))
            {
                return repository.selectFirst(p => p.Usuario == userId);
            }
        }

        public Task<UsuarioT5> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedUserName))
            {
                throw new ArgumentNullException("normalizedUserName");
            }

            return this._session.QueryOver<UsuarioT5>()
                                  .WhereRestrictionOn(x => x.Usuario).IsInsensitiveLike(normalizedUserName)
                                  .SingleOrDefaultAsync();
        }
       

        public Task<string> GetNormalizedUserNameAsync(UsuarioT5 user, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.Usuario.ToUpper(), cancellationToken);
        }

        public Task<string> GetUserIdAsync(UsuarioT5 user, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.Usuario, cancellationToken);
        }

        public Task<string> GetUserNameAsync(UsuarioT5 user, CancellationToken cancellationToken)
        {
            return Task.Run(() => user.Usuario, cancellationToken);
        }

        // Atualmente não temos um campo para guardar a informação do nome do usuário normalizado (todo maiúsculo).
        // Não sei se isso gera algum problema
        public Task SetNormalizedUserNameAsync(UsuarioT5 user, string normalizedName, CancellationToken cancellationToken)
        {
            //throw new System.NotImplementedException();
            return Task.CompletedTask;
        }

        // Coloquei Update pois acho que esse método só será chamado quando o usuário já estiver gravado em banco.
        public async Task SetUserNameAsync(UsuarioT5 user, string userName, CancellationToken cancellationToken)
        {
            using (var repository = new Conexao<UsuarioT5>(_session))
            {
                user.Usuario = userName;
                await repository.Update(user);
            }
        }

        public async Task<IdentityResult> UpdateAsync(UsuarioT5 user, CancellationToken cancellationToken)
        {
            try
            {
                using (var repository = new Conexao<UsuarioT5>(_session))
                {
                    await repository.Update(user);
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }
    }
}
