using Microsoft.Extensions.DependencyInjection;

namespace Aniel.Connect.Core.AspNetCore.Config.Session
{
    public static class SessionConfigurationExtensions
    {
        /// <summary>
        ///     Adiciona o SessionResolver na aplicação, para poder se conectar com diferentes bancos de dados
        /// </summary>
        public static void AddSessionResolver(this IServiceCollection services)
        {
            services.AddSingleton<SessionResolverFunc>(serviceProvider => SessionResolver.Resolve);
        }
    }
}
