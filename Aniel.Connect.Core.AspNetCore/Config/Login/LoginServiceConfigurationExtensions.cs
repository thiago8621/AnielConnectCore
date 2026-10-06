using Microsoft.Extensions.DependencyInjection;
using Sinapse.Model.AspNetCore;

namespace Aniel.Connect.Core.AspNetCore.Config.Login
{
    public static class LoginServiceConfigurationExtensions
    {
        /// <summary>
        ///     Adiciona a configuração necessária na aplicação para utilizar a TB_SENHA para a gestão dos usuários do sistema
        /// </summary>
        public static void AddLoginAnielUser(this IServiceCollection services)
        {
            services
                .AddDefaultIdentity<UsuarioT5>()
                .AddUserStore<AnielUserStore>();
        }
      
    }
}
