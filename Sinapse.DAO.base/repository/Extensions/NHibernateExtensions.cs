using NHibernate.Engine;
using NHibernate.Hql;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.DAO.repository
{
    public static class NHibernateExtensions
    {
        /// <summary>
        ///     Extrai o SQL de uma consulta do NHibernate feita com LINQ.
        ///     <br /> <br />
        ///     Referência: <see href="https://gunnarpeipman.com/nhibernate-linq-tosql/"/>
        /// </summary>
        /// <param name="query">Consulta do NHibernate feita com LINQ que será analisada</param>
        public static string ToSql(this IQueryable query)
        {
            var provider = query.Provider as DefaultQueryProvider;

            if (provider == null)
                return null;

            ISessionImplementor sessionImpl = provider.Session;
            ISessionFactoryImplementor factory = sessionImpl.Factory;

            var linqExpression = new NhLinqExpression(query.Expression, factory);
            var translatorFactory = new ASTQueryTranslatorFactory();

            IQueryTranslator translator = 
                translatorFactory.CreateQueryTranslators(
                    linqExpression,
                    null,
                    false,
                    sessionImpl.EnabledFilters,
                    factory
                ).FirstOrDefault();

            if (translator == null)
                return String.Empty;

            string sql = translator.SQLString;

            IEnumerable<string> parameters = linqExpression.ParameterDescriptors.Select(p => p.Name).Distinct();

            if (parameters.Count() > 0)
                sql += "\r\n\r\nParâmetros:\r\n";

            foreach (string param in parameters)
            {
                //Appends a string in the following format: "(query parameter): (parameter value)"
                sql += "\r\n" + param + ": " + linqExpression.ParameterValuesByName[param].Item1;
            }

            return sql.Trim();
        }
    }
}
