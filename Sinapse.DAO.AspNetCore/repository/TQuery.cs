using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Sinapse.DAO.AspNetCore.repository
{
    /// <summary>
    /// Query "PQuery" feita com Transação
    /// </summary>
    /// <typeparam name="T">Classe a ser preenchida</typeparam>
    public class TQuery<T> where T : class
    {
        private TQConexao trans;
        public TQuery(Transaction<T> t, Expression<Func<T, bool>> expression)
        {
            trans = new TQConexao(t);

            if (expression != null) trans.Query(expression);
        }

        public TQuery<T> And(Expression<Func<T, bool>> expression)
        {
            trans.And(expression);
            return this;
        }

        public List<T> Select()
        {
            List<T> retono = new List<T>();

            retono = new List<T>(trans.Select());

            return retono;
        }

        public TQuery<T> join<J>(Expression<Func<T, J>> join, Expression<Func<J, bool>> expression)
        {
            trans.getvQuery().JoinQueryOver(join).Where(expression);
            return this;
        }

        private class TQConexao
        {            
            private NHibernate.IQueryOver<T, T> query;
            private ISession _session;

            public TQConexao(ISession session)
            {
                _session = session;
            }

            public TQConexao(Transaction<T> t)
            {                
                query = _session.QueryOver<T>();
            }

            public void Query(Expression<Func<T, bool>> expression)
            {
                query.Where(expression);
            }

            public void And(Expression<Func<T, bool>> expression)
            {
                query.And(expression);
            }

            public IList<T> Select()
            {
                return query.List();
            }

            public NHibernate.IQueryOver<T, T> getvQuery()
            {
                return query;
            }
        }
    }
}
