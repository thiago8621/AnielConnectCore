using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sinapse.DAO.AspNetCore.repository
{
    /// <summary>
    /// Query com select feito e gerenciado pelo NHibarnate
    /// </summary>
    /// <typeparam name="T">Classe a ser preenchida</typeparam>
    public class PQuery<T> : IDisposable where T : class
    {
        private PConexao conexao;
        private ISession _session;

        public PQuery(ISession session)
        {
            _session = session;
        }

        public PQuery(Expression<Func<T, bool>> expression)
        {
            conexao = new PConexao(_session);
            conexao.Open();

            if (expression != null) conexao.Query(expression);
        }

        public PQuery()
        {
            conexao = new PConexao(_session);
            conexao.Open();
        }

        public PQuery<T> And(Expression<Func<T, bool>> expression)
        {
            conexao.And(expression);
            return this;
        }

        public PQuery<T> In(object[] values, params Expression<Func<T, object>>[] properts)
        {
            List<IProjection> projections = new List<IProjection>();
            foreach (Expression<Func<T, object>> cp in properts)
                projections.Add(Projections.Property<T>(cp));

            IProjection propriedades = Projections.SqlFunction("concat", NHibernateUtil.String, projections.ToArray());

            conexao.In(propriedades, values);
            return this;
        }

        public PQuery<T> Like(string property, string valor)
        {
            this.Like(new string[] { property }, valor, EModoBusca.Anywhere);
            return this;
        }

        public PQuery<T> Like(string[] properties, string valor)
        {
            this.Like(properties, valor, EModoBusca.Anywhere);
            return this;
        }

        public PQuery<T> Like(string[] property, string valor, EModoBusca modoBusca)
        {
            MatchMode matchMode = null;
            switch (modoBusca)
            {
                case EModoBusca.Exact: matchMode = MatchMode.Exact; break;
                case EModoBusca.Start: matchMode = MatchMode.Start; break;
                case EModoBusca.Anywhere: matchMode = MatchMode.Anywhere; break;
                case EModoBusca.End: matchMode = MatchMode.End; break;
            }

            conexao.Like(valor, matchMode, property);
            return this;
        }

        public List<T> Select()
        {
            List<T> retono = new List<T>();

            retono = new List<T>(conexao.Select());

            return retono;
        }

        public List<T> Select(Int32 pTake, Int32 pSkip)
        {
            List<T> retono = new List<T>();

            retono = new List<T>(conexao.Select(pTake, pSkip));

            return retono;
        }

        public PQuery<T> join<J>(Expression<Func<T, J>> join, Expression<Func<J, bool>> expression)
        {
            conexao.getvQuery().JoinQueryOver(join).Where(expression);
            return this;
        }

        #region Disposable
        bool disposed = false;

        // método privado para controle
        // da liberação dos recursos
        private void Dispose(bool disposing)
        {
            // Verifique se Dispose já foi chamado.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Liberando recursos gerenciados

                }

                // Seta a variável booleana para true,
                // indicando que os recursos já foram liberados
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        private class PConexao : Conexao<T>
        {
            private NHibernate.IQueryOver<T, T> vQuery;
            private ISession _session;

            public PConexao(ISession session) : base (session)
            {
                _session = session;
            }

            public void Open()
            {
                vQuery = _session.QueryOver<T>();
            }

            public void Query(Expression<Func<T, bool>> expression)
            {
                vQuery.Where(expression);
            }

            public void And(Expression<Func<T, bool>> expression)
            {
                vQuery.And(expression);
            }

            public void In(IProjection property, object[] values)
            {
                vQuery.And(Restrictions.In(property, values));
            }

            public void Like(string value, MatchMode modoBusca, params string[] property)
            {
                var properties = (from p in property
                                  select Projections.Property(p)).ToArray();

                vQuery.And(Restrictions.Like(Projections.SqlFunction("concat", NHibernateUtil.String, properties), value, modoBusca));
            }

            public IList<T> Select()
            {
                return vQuery.List();
            }

            public IList<T> Select(Int32 pTake, Int32 pSkip)
            {
                return vQuery.Take(pTake).Skip(pSkip).List();
            }

            public NHibernate.IQueryOver<T, T> getvQuery()
            {
                return vQuery;
            }

            
        }
    }
}
