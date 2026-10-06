using FluentNHibernate.Mapping;
using Sinapse.Model.AspNetCore.enumerados;
using Sinapse.ModelLog.AspNetCore;

namespace Sinapse.DAOLog.AspNetCore
{
    /// <typeparam name="T">Classe modelo</typeparam>
    /// <typeparam name="Y">Tipo do Campo IdLog</typeparam>
    public class LogGenericoMap<T, Y> : ClassMap<T> where T : ILogGenerico<Y>
    {
        public LogGenericoMap() : base()
        {
            Map(p => p.UsuarioCriacao).Length(30);
            Map(p => p.NomeUsuarioCriacao);
            Map(p => p.DataHoraCriacao);
            Map(p => p.TipoOperacao, "TIPOOPERACAO").CustomType<TipoOperacao>();

            Map(p => p.IdLog, "IDLOG");

            Map(p => p.Campo_ValorAntes, "CAMPO_VALORANTES");
            Map(p => p.Campo_ValorDepois, "CAMPO_VALORDEPOIS");
        }


    }
}
