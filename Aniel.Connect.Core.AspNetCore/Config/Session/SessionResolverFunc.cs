using NHibernate;

namespace Aniel.Connect.Core.AspNetCore.Config.Session
{
    /// <summary>
    ///     Representa uma função que retorna a session adequada para o tipo de session requisitada
    /// </summary>
    /// <param name="type">Tipo da session que deverá ser retornada</param>
    public delegate ISession SessionResolverFunc(SessionType type);
}
