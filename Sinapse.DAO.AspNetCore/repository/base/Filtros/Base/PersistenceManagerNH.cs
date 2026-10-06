using NHibernate;
using NHibernate.Criterion;
using Sinapse.DAO.AspNetCore.repository;
using Sinapse.Model.AspNetCore.core;
using Sinapse.Model.AspNetCore.nhibernate;
using Sinapse.Util.AspNetCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace Sinapse.DAO.AspNetCore
{
    public class PersistenceManagerNH : IPersistenceManager, IDisposable
    {
        private IPersistenceFactory persistenceFactory = PersistenceAbstractFactory.GetPersistenceFactory();

        const Int32 MAXIMOREGISTROSCONSULTA = 15000;

        public ISession GetSession()
        {
            try
            {
                ISession retorno = HibernateUtil.GetSessionFactory().Result.OpenSession();
                retorno.FlushMode = FlushMode.Always;
                //retorno.BeginTransaction();
                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro para recuperar sessão " + ex.Message);
            }
        }

        public IStatelessSession GetStatelessSession()
        {
            try
            {
                return SessionFactoryManager.GetSessionFactory().OpenStatelessSession();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro para recuperar sessão Stateless", ex);
            }
        }

        public void SetFlushMode(FlushMode pFlushMode)
        {
            GetSession().FlushMode = pFlushMode;
        }

        public ITransaction BeginTransaction()
        {
            return GetSession().BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public ITransaction GetTransaction()
        {
            return GetSession().Transaction;
        }

        public bool InsertBatch<T>(T obj) where T : IModel
        {
            IStatelessSession session = GetStatelessSession();

            try
            {
                session.Insert(obj);
            }
            catch (Exception e)
            {
                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message, e, obj);
            }

            session.Close();

            return true;
        }

        public bool UpdateBatch<T>(T obj) where T : IModel
        {
            IStatelessSession session = GetStatelessSession();

            try
            {
                session.Update(obj);
            }
            catch (Exception e)
            {
                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            session.Close();

            return true;
        }

        public bool InsertOrUpdate<T>(T obj) where T : IModel
        {
            ISession session = GetSession();

            ITransaction transaction = BeginTransaction();

            try
            {
                //ModelValidatorService.ValidateModel(obj, transaction);

                session.SaveOrUpdate(obj);

                transaction.Commit();
            }
            /*catch (AnielInnovationException ex)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                session.Clear();

                throw ex;
            }*/
            catch (Exception e)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                session.Clear();

                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }

        public bool InsertOrUpdate(Type tipo, object obj)
        {
            ISession session = GetSession();
            ITransaction transaction = BeginTransaction();

            try
            {
                //ModelValidatorService.ValidateModel(obj, transaction);

                session.SaveOrUpdate(tipo.AssemblyQualifiedName, obj);

                transaction.Commit();
            }
            /*catch (AnielInnovationException ex)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                session.Clear();

                throw ex;
            }*/
            catch (Exception e)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                session.Clear();

                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }

        public bool Save(Type tipo, object obj)
        {
            ISession session = GetSession();
            ITransaction transaction = BeginTransaction();

            try
            {
                //ModelValidatorService.ValidateModel(obj, transaction);

                session.Save(tipo.AssemblyQualifiedName, obj);

                transaction.Commit();
            }
            /*catch (AnielInnovationException ex)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                throw ex;
            }*/
            catch (Exception e)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }
            return true;
        }

        public bool Update(Type tipo, object obj)
        {
            ISession session = GetSession();
            ITransaction transaction = BeginTransaction();

            try
            {
                //ModelValidatorService.ValidateModel(obj, transaction);

                session.Update(tipo.AssemblyQualifiedName, obj);

                transaction.Commit();
            }
            /*catch (AnielInnovationException ex)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                throw ex;
            }*/
            catch (Exception e)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }
            return true;
        }

        public bool Delete<T>(T obj) where T : IModel
        {
            ISession session = GetSession();
            ITransaction transaction = BeginTransaction();

            try
            {
                session.Delete(obj);

                transaction.Commit();
            }
            catch (Exception e)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                session.Clear();

                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }

        public bool DeleteObject(object obj)
        {
            ISession session = GetSession();
            ITransaction transaction = BeginTransaction();

            try
            {
                session.Delete(obj);

                transaction.Commit();

                session.Flush();

            }
            catch (Exception e)
            {
                if (transaction.IsActive)
                    transaction.Rollback();

                session.Clear();

                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }

        public object GetById(Type tipo, long oid)
        {
            object obj = null;
            ISession session = GetSession();

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                obj = session.Get(tipo, oid);

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return obj;
        }

        public object GetByIdStateless(Type tipo, long oid)
        {
            object obj = null;
            IStatelessSession session = GetStatelessSession();

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                obj = session.Get(tipo.FullName, oid);

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            session.Close();

            return obj;
        }

        public T GetById<T>(long oid) where T : IModel
        {
            T obj;
            ISession session = GetSession();

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                obj = session.Get<T>(oid);

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return obj;

        }

        public T GetByIdEagger<T>(long oid, String propertyEager) where T : class, IModel
        {
            T obj;
            ISession session = GetSession();

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                obj = session.CreateCriteria<T>().Add(Expression.IdEq(oid)).Fetch(SelectMode.Fetch, propertyEager).UniqueResult<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return obj;
        }

        public IList<T> GetAll<T>() where T : IModel
        {
            IList<T> lista = null;
            ISession session = GetSession();
            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                ICriteria criteria = session.CreateCriteria(typeof(T));
                
                lista = criteria.SetMaxResults(MAXIMOREGISTROSCONSULTA).List<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return lista;
        }

        public IList<T> GetAllWithoutToken<T>() where T : IModel
        {
            IList<T> lista = null;
            ISession session = GetSession();
            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                ICriteria criteria = session.CreateCriteria(typeof(T));

                lista = criteria.SetMaxResults(MAXIMOREGISTROSCONSULTA).List<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return lista;
        }


        public IList<T> GetListByExample<T>(T objEx, int maxResults) where T : IModel
        {
            ISession session = GetSession();
            IList<T> lista = null;

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                ICriteria criteria = session.CreateCriteria(typeof(T)).Add(GetExcludeProperties(objEx)).SetMaxResults(maxResults);

                lista = criteria.List<T>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return lista;
        }

        public IList<T> GetListByExample<T>(T objEx) where T : IModel
        {
            ISession session = GetSession();
            IList<T> lista = null;

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                ICriteria criteria = GenerateExampleCriteria(objEx);
                
                lista = criteria.List<T>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }
            return lista;
        }

        public T GetObjectByExample<T>(T objEx) where T : IModel
        {
            T obj;
            ICriteria criterioBusca = GenerateExampleCriteria(objEx);

            //ITransaction transacao = GetSession().BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                obj = criterioBusca.UniqueResult<T>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();

                throw ex;
            }

            return obj;
        }

        public T GetObjectByField<T>(String fieldName, Object value) where T : IModel
        {
            T objRetorno = default(T);

            ISession session = GetSession();

            //ITransaction transaction = null;
            ICriteria criterio = null;

            //transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            if (value == null)
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.IsNull(fieldName));
            else
            {
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.Eq(fieldName, value).IgnoreCase());
            }

            try
            {
                
                IList<T> listaRetorno = criterio.List<T>();

                if (listaRetorno.Count > 0)
                    objRetorno = listaRetorno[0];

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return objRetorno;

        }

        public T GetObjectByUnikeyField<T>(String fieldName, Object value) where T : IModel
        {
            T objRetorno;

            ISession session = GetSession();

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            ICriteria criterio = null;

            if (value == null)
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.IsNull(fieldName));
            else
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.Eq(fieldName, value).IgnoreCase());

            try
            {
                objRetorno = criterio.UniqueResult<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return objRetorno;
        }


        public T GetObjectByUnikeField<T>(String fieldName, Object value) where T : IModel
        {
            T objRetorno;

            ISession session = GetSession();

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);

            ICriteria criterio = null;

            if (value == null)
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.IsNull(fieldName));
            else
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.Eq(fieldName, value).IgnoreCase());

            try
            {
                objRetorno = criterio.UniqueResult<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return objRetorno;
        }

        public IList<T> GetListByField<T>(String fieldName, Object value) where T : IModel
        {
            ISession session = GetSession();

            if (session == null)
                throw new Exception("Sessão nula");

            IList<T> listQry = null;
            ICriteria criteria = session.CreateCriteria(typeof(T));

            if (criteria == null)
                throw new Exception("Criteria nula");

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                if (value != null)
                    criteria = criteria.Add(Expression.Eq(fieldName, value));
                else
                    criteria = criteria.Add(Expression.IsNull(fieldName));

                listQry = criteria.List<T>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return listQry;
        }

        public IList<T> GetListByField<T>(String fieldName, Object value, SearchType searchType) where T : IModel
        {
            ISession session = GetSession();
            IList<T> listQry = null;

            ICriteria criteria = session.CreateCriteria(typeof(T));

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                if (searchType == SearchType.Equal)
                {
                    if (value == null)
                        criteria = criteria.Add(Expression.IsNull(fieldName));
                    else if (value.GetType() == typeof(String))
                        criteria = criteria.Add(Expression.Eq(fieldName, value).IgnoreCase());
                    else
                        criteria = criteria.Add(Expression.Eq(fieldName, value));
                }
                else if (searchType == SearchType.Contains)
                {
                    criteria = criteria.Add(Expression.InsensitiveLike(fieldName, value.ToString(), MatchMode.Anywhere));
                }
                else if (searchType == SearchType.BeginWith)
                {
                    criteria = criteria.Add(Expression.InsensitiveLike(fieldName, value.ToString(), MatchMode.Start));
                }

                listQry = criteria.List<T>();

            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return listQry;
        }

        public IList<T> GetListByField<T>(String fieldName, Object value, SearchType searchType, int MaxResult) where T : IModel
        {
            ISession session = GetSession();
            IList<T> listQry = null;

            ICriteria criteria = session.CreateCriteria(typeof(T));

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {

                if (value == null)
                {
                    criteria = criteria.Add(Expression.IsNull(fieldName)).SetMaxResults(MaxResult);
                }
                if (searchType == SearchType.Equal)
                {
                    if (value == null)
                        criteria = criteria.Add(Expression.IsNull(fieldName));
                    else if (value.GetType() == typeof(String))
                        criteria = criteria.Add(Expression.Eq(fieldName, value).IgnoreCase()).SetMaxResults(MaxResult);
                    else
                        criteria = criteria.Add(Expression.Eq(fieldName, value)).SetMaxResults(MaxResult);
                }
                else if (searchType == SearchType.Contains)
                {
                    criteria = criteria.Add(Expression.InsensitiveLike(fieldName, value.ToString(), MatchMode.Anywhere)).SetMaxResults(MaxResult);
                }
                else if (searchType == SearchType.BeginWith)
                {
                    criteria = criteria.Add(Expression.InsensitiveLike(fieldName, value.ToString(), MatchMode.Start)).SetMaxResults(MaxResult);
                }

                listQry = criteria.List<T>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }
            return listQry;
        }

        public IList<object> GetListByField(Type tipo, String fieldName, Object value, SearchType searchType)
        {
            ISession session = GetSession();
            IList<object> listQry = null;

            ICriteria criteria = session.CreateCriteria(tipo);

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                AddCriteriaExpression(criteria, fieldName, value, searchType);

                listQry = criteria.List<object>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return listQry;
        }

        public object GetObjectByField(Type tipo, String fieldName, Object value)
        {
            ISession session = GetSession();
            object retorno = null;

            ICriteria criteria = session.CreateCriteria(tipo);

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);

            try
            {
                AddCriteriaExpression(criteria, fieldName, value, SearchType.Equal);

                retorno = criteria.UniqueResult();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return retorno;
        }



        public IList<T> GetOrdedListByExample<T>(T objEx, OrdenacaoLista ordem, params string[] fieldNames) where T : IModel
        {
            ISession session = GetSession();
            IList<T> lista;
            ICriteria criterioBusca = GenerateExampleCriteria<T>(objEx);

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                foreach (string str in fieldNames)
                {
                    if (ordem == OrdenacaoLista.Ascendente)
                        criterioBusca.AddOrder(Order.Asc(str));
                    else
                        criterioBusca.AddOrder(Order.Desc(str));
                }

                lista = criterioBusca.List<T>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return lista;
        }

        public IList<object> GetList(Type tipo, int MaxResults)
        {
            ISession session = GetSession();
            IList<object> listQry = null;

            ICriteria criteria = session.CreateCriteria(tipo).SetMaxResults(MaxResults);

            //ITransaction transacao = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                listQry = criteria.List<object>();

                //transacao.Commit();
            }
            catch (Exception ex)
            {
                //if (transacao.IsActive)
                //    transacao.Rollback();
                throw ex;
            }

            return listQry;
        }

        private IList<T> GetListByFiltro<T>(Filters filtros, FetchMode mode, String EaggerProperty) where T : IModel
        {
            ISession session = GetSession();
            IList<T> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(typeof(T));

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                FilterToCriteria<T>(criterio, filtros);

                if (mode != FetchMode.Default)
                    lstRetorno = criterio.Fetch(SelectMode.Fetch, EaggerProperty).List<T>();
                else
                    lstRetorno = criterio.List<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return lstRetorno;
        }
               
        
        private IList<Object> GetListByFiltro(Type tipo, Filters filtros, FetchMode mode, String EaggerProperty)
        {
            ISession session = GetSession();
            IList<Object> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(tipo);

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                FilterToCriteria(tipo, criterio, filtros);

                if (mode != FetchMode.Default)
                    lstRetorno = criterio.Fetch(SelectMode.Fetch, EaggerProperty).List<Object>();
                else
                    lstRetorno = criterio.List<Object>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return lstRetorno;
        }

        public IList<T> GetListByFiltroPage<T>(Filters filtros, Int32 pageIndex, Int32 pageSize) where T : IModel
        {
            ISession session = GetSession();
            IList<T> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(typeof(T));

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {

                FilterToCriteria<T>(criterio, filtros);

                lstRetorno = criterio.SetFirstResult(pageIndex - 1 * pageSize).SetMaxResults(pageSize).List<T>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return lstRetorno;
        }

        
        public IList<Object> GetListByFiltroPage(Type tipo, Filters filtros, Int32 pageIndex, Int32 pageSize)
        {
            ISession session = GetSession();
            IList<Object> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(tipo);

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                FilterToCriteria(tipo, criterio, filtros);

                lstRetorno = criterio.SetFirstResult((pageIndex - 1) * pageSize).SetMaxResults(pageSize).List<Object>();

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return lstRetorno;
        }

        public T GetObjectByFiltro<T>(Filters filtros, FetchMode mode, String EaggerProperty) where T : IModel
        {
            ISession session = GetSession();
            T retorno = default(T);

            ICriteria criterio = session.CreateCriteria(typeof(T));

            //ITransaction transaction = session.BeginTransaction(IsolationLevel.Unspecified);
            try
            {
                FilterToCriteria<T>(criterio, filtros);

                if (mode != FetchMode.Default)
                    retorno = criterio.Fetch(SelectMode.Fetch, EaggerProperty).UniqueResult<T>();
                else
                {
                    IList<T> lista = criterio.List<T>();
                    if (lista.Count == 0)
                        retorno = default(T);
                    else
                        retorno = lista[0];
                }
                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //if (transaction.IsActive)
                //    transaction.Rollback();
                throw ex;
            }

            return retorno;
        }


        //public IList<T> GetListByFiltroEagger<T>(Filters filtros, String EaggerProperty) where T : IModel
        //{
        //    return GetListByFiltro<T>(filtros, FetchMode.Eager, EaggerProperty);
        //}

        public IList<T> GetListByFiltro<T>(Filters filtros) where T : IModel
        {
            return GetListByFiltro<T>(filtros, FetchMode.Default, "-");
        }

        public IList<Object> GetListByFiltro(Type tipo, Filters filtros)
        {
            return GetListByFiltro(tipo, filtros, FetchMode.Default, "-");
        }

        public T GetObjectByFiltro<T>(Filters filtros) where T : IModel
        {
            return GetObjectByFiltro<T>(filtros, FetchMode.Default, "-");
        }

        //METODOS SEM ABERTURA EXPLICITA DE TRANSACAO - EXECUTADOS EM OPERACOES EM LOTE
        //Estes metodos assumes que a transação foi aberta anteriormente

        public IList<object> GetListByFieldInTransaction(Type tipo, string fieldName, object value, SearchType searchType)
        {
            ISession session = GetSession();
            IList<object> listQry = null;

            ICriteria criteria = session.CreateCriteria(tipo);

            try
            {
                AddCriteriaExpression(criteria, fieldName, value, searchType);

                listQry = criteria.List<object>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listQry;
        }


        //Deleta um objeto sem startar e comitar a transacao
        //pois assume que a operacao foi executada dentro do contexto de outra transacao
        public bool InsertOrUpdateInTransaction(Type tipo, Object obj)
        {
            ISession session = GetSession();
            try
            {
                //ModelValidatorService.ValidateModel(obj, session.Transaction);

                session.SaveOrUpdate(tipo.AssemblyQualifiedName, obj);
            }
            /*catch (AnielInnovationException ex)
            {
                throw ex;
            }*/
            catch (Exception e)
            {
                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }


        //Deleta um objeto sem startar e comitar a transacao
        //pois assume que a operacao foi executada dentro do contexto de outra transacao
        public bool DeleteInTransaction(object obj)
        {
            ISession session = GetSession();

            try
            {
                session.Delete(obj);

            }
            catch (Exception e)
            {
                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }

        //METODOS DE ESCOPO PRIVADO
        /*
        private void AddTokenCriteria(ICriteria criteria, Type tipo)
        {
            try
            {
                if (!IgnoredTokens.Tokens.Contains(tipo))
                {
                    if (this.GetUsuarioLogado() == null)
                    {
                        if (ModelAttributeService.GetTokenLessAtt(tipo) == null)
                            throw new SessionExpiredException();
                    }
                    else
                    {
                        if (ModelAttributeService.GetTokenLessAtt(tipo) == null)
                            criteria = criteria.Add(Expression.Eq("Token", this.GetToken()));
                    }
                }
            }
            catch
            {
                throw new Exception("Erro no AddToken");
            }
        }*/

        
        private Example GetExcludeProperties<T>(T obj) where T : IModel
        {
            Example ex = Example.Create(obj);
            ex.ExcludeNulls();
            ex.ExcludeZeroes();

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(DateTime))
                {
                    if ((prop.GetValue(obj, null) == null) ||
                        (Convert.ToDateTime(prop.GetValue(obj, null)) == DateTime.MinValue))
                    {
                        ex.ExcludeProperty(prop.Name);
                    }
                }
                if (prop.PropertyType == typeof(Int32))
                {
                    if ((prop.GetValue(obj, null) == null) ||
                        (Convert.ToInt32(prop.GetValue(obj, null)) == 0))
                    {
                        ex.ExcludeProperty(prop.Name);
                    }
                }
                if ((prop.PropertyType == typeof(Int64)) || (prop.PropertyType == typeof(long)))
                {
                    if ((prop.GetValue(obj, null) == null) ||
                        (Convert.ToInt64(prop.GetValue(obj, null)) == 0))
                    {
                        ex.ExcludeProperty(prop.Name);
                    }
                }
                else
                {
                    if (prop.GetValue(obj, null) == null)
                    {
                        ex.ExcludeProperty(prop.Name);
                    }
                }
            }
            return ex;
        }

        private ICriteria GenerateExampleCriteria<T>(T model) where T : IModel
        {
            ISession session = GetSession();

            ICriteria criterio = session.CreateCriteria(typeof(T)).Add(GetExcludeProperties(model));

            PropertyInfo[] props = model.GetType().GetProperties();

            foreach (PropertyInfo p in props)
            {
                if ((p.PropertyType.IsClass) &&
                    typeof(IModel).IsAssignableFrom(p.PropertyType))
                {
                    if (p.GetValue(model, null) != null)
                    {
                        IModel valorAssociado = (IModel)p.GetValue(model, null);

                        criterio.CreateCriteria(p.Name).Add(Expression.Eq("Id", valorAssociado.Id));
                    }
                }
            }

            return criterio;
        }

        private void AddAssociatedCriteria<T>(ICriteria criterioBase, String ParentName, String FieldName, Filter filtro) where T : IModel
        {
            PropertyInfo[] props = typeof(T).GetProperties();

            foreach (PropertyInfo p in props)
            {
                if ((p.PropertyType.IsClass || p.PropertyType.IsInterface) &&
                    (p.PropertyType.GetInterface(typeof(IModel).ToString()) != null)
                    && (p.Name.ToUpper() == ParentName.ToUpper()))
                {
                    PropertyInfo[] propertyParent = p.PropertyType.GetProperties();

                    foreach (PropertyInfo pp in propertyParent)
                    {
                        if (((pp.PropertyType.GetInterface(typeof(IModel).ToString()) == null) &&
                             (pp.Name.ToUpper() == FieldName.ToUpper())) ||

                            ((pp.PropertyType.GetInterface(typeof(IModel).ToString()) != null) &&
                                 (FieldName.ToUpper().Contains(pp.Name.ToUpper())) && (!FieldName.Contains("."))))
                        {
                            ICriteria criterioAssociado = criterioBase.GetCriteriaByPath(p.Name);

                            if (criterioAssociado == null)
                                criterioAssociado = criterioBase.CreateCriteria(p.Name);

                            switch (filtro.Expressao)
                            {
                                case SearchType.Equal:
                                    if (filtro.GetUniqueValue() == null)
                                        criterioAssociado.Add(Expression.IsNull(FieldName));
                                    else if (filtro.GetUniqueValue().GetType() == typeof(String))
                                        criterioAssociado.Add(Expression.Eq(FieldName, filtro.GetUniqueValue()).IgnoreCase());
                                    else
                                        criterioAssociado.Add(Expression.Eq(FieldName, filtro.GetUniqueValue()));
                                    break;

                                case SearchType.NotEqual:
                                    if (filtro.GetUniqueValue() == null)
                                        criterioAssociado.Add(Expression.IsNotNull(FieldName));
                                    else if (filtro.GetUniqueValue().GetType() == typeof(String))
                                        criterioAssociado.Add(Expression.Not(Expression.Eq(FieldName, filtro.GetUniqueValue()).IgnoreCase()));
                                    else
                                        criterioAssociado.Add(Expression.Not(Expression.Eq(FieldName, filtro.GetUniqueValue())));

                                    break;

                                case SearchType.Contains:
                                    criterioAssociado.Add(Expression.InsensitiveLike(FieldName, filtro.GetContains().ToString(), MatchMode.Anywhere));
                                    break;

                                case SearchType.BeginWith:
                                    criterioAssociado.Add(Expression.InsensitiveLike(FieldName, filtro.GetBeginWithValue().ToString(), MatchMode.Start));
                                    break;
                            }
                        }
                        else if ((pp.PropertyType.GetInterface(typeof(IModel).ToString()) != null) &&
                                 (FieldName.ToUpper().Contains(pp.Name.ToUpper())))
                        {
                            if (FieldName.Contains("."))
                            {
                                String parent = FieldName.Substring(0, FieldName.IndexOf("."));

                                String child = FieldName.Replace(parent + ".", null);

                                ICriteria criterioAssociado = criterioBase.GetCriteriaByPath(p.Name);

                                if (criterioAssociado == null)
                                    criterioAssociado = criterioBase.CreateCriteria(p.Name);

                                criterioAssociado.Fetch(SelectMode.Fetch, pp.PropertyType.Name);

                                AddAssociatedCriteria(p.PropertyType, ref criterioAssociado, parent, child, filtro);
                            }
                        }
                    }
                }
            }
        }

        private void AddAssociatedCriteria(Type tipo, ref ICriteria criterioBase, String ParentName, String FieldName, Filter filtro)
        {
            PropertyInfo[] props = tipo.GetProperties();

            foreach (PropertyInfo p in props)
            {
                if ((p.PropertyType.IsClass) &&
                    ((p.PropertyType.GetInterface(typeof(IModel).ToString()) != null) ||
                     (p.PropertyType.GetInterface(typeof(ILegado).ToString()) != null))
                    && (p.Name.ToUpper() == ParentName.ToUpper()))
                {
                    PropertyInfo[] propertyParent = p.PropertyType.GetProperties();

                    foreach (PropertyInfo pp in propertyParent)
                    {
                        if (((pp.PropertyType.GetInterface(typeof(IModel).ToString()) == null) ||
                              (pp.PropertyType.GetInterface(typeof(ILegado).ToString()) == null))
                            && (pp.Name.ToUpper() == FieldName.ToUpper()))
                        {
                            ICriteria criterioAssociado = criterioBase.GetCriteriaByPath(p.Name);

                            if (criterioAssociado == null)
                                criterioAssociado = criterioBase.CreateCriteria(ParentName);

                            switch (filtro.Expressao)
                            {
                                case SearchType.Equal:
                                    if (filtro.GetUniqueValue() == null)
                                        criterioAssociado.Add(Expression.IsNull(FieldName));
                                    else if (filtro.GetUniqueValue().GetType() == typeof(String))
                                        criterioAssociado.Add(Expression.Eq(FieldName, filtro.GetUniqueValue()).IgnoreCase());
                                    else
                                        criterioAssociado.Add(Expression.Eq(FieldName, filtro.GetUniqueValue()));
                                    break;

                                case SearchType.NotEqual:
                                    if (filtro.GetUniqueValue() == null)
                                        criterioAssociado.Add(Expression.IsNotNull(FieldName));
                                    else if (filtro.GetUniqueValue().GetType() == typeof(String))
                                        criterioAssociado.Add(Expression.Not(Expression.Eq(FieldName, filtro.GetUniqueValue()).IgnoreCase()));
                                    else
                                        criterioAssociado.Add(Expression.Not(Expression.Eq(FieldName, filtro.GetUniqueValue())));
                                    break;

                                case SearchType.Contains:
                                    criterioAssociado.Add(Expression.InsensitiveLike(FieldName, filtro.GetContains().ToString(), MatchMode.Anywhere));
                                    break;

                                case SearchType.BeginWith:
                                    criterioAssociado.Add(Expression.InsensitiveLike(FieldName, filtro.GetBeginWithValue().ToString(), MatchMode.Start));
                                    break;
                            }
                        }
                        else if (((pp.PropertyType.GetInterface(typeof(IModel).ToString()) != null) ||
                                  (pp.PropertyType.GetInterface(typeof(ILegado).ToString()) != null))
                                && (FieldName.ToUpper().Contains(pp.Name.ToUpper())))
                        {
                            if (FieldName.Contains("."))
                            {
                                String parent = FieldName.Substring(0, FieldName.IndexOf("."));

                                String child = FieldName.Replace(parent + ".", null);

                                ICriteria criterioAssociado = criterioBase.GetCriteriaByPath(p.Name);

                                if (criterioAssociado == null)
                                    criterioAssociado = criterioBase.CreateCriteria(p.Name);

                                criterioAssociado.Fetch(pp.PropertyType.Name, pp.PropertyType.Name);

                                AddAssociatedCriteria(p.PropertyType, ref criterioAssociado, parent, child, filtro);
                            }
                        }
                    }

                }
            }
        }


        public void FilterToCriteria<T>(ICriteria criterio, Filters filtros) where T : IModel
        {
            foreach (Filter filter in filtros.GetFilterList())
            {
                if (!filter.NomeCampo.Contains("."))
                {
                    switch (filter.Expressao)
                    {
                        case SearchType.Equal:
                            {
                                if (filter.GetUniqueValue() == null)
                                    criterio = criterio.Add(Expression.IsNull(filter.NomeCampo));
                                else if (filter.GetUniqueValue().GetType() == typeof(String))
                                    criterio = criterio.Add(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue()).IgnoreCase());
                                else
                                    criterio = criterio.Add(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue()));
                            }
                            break;

                        case SearchType.NotEqual:
                            {
                                if (filter.GetUniqueValue() == null)
                                    criterio = criterio.Add(Expression.IsNotNull(filter.NomeCampo));
                                else if (filter.GetUniqueValue().GetType() == typeof(String))
                                    criterio = criterio.Add(Expression.Not(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue()).IgnoreCase()));
                                else
                                    criterio = criterio.Add(Expression.Not(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue())));

                            }
                            break;

                        case SearchType.Between:
                            {
                                criterio = criterio.Add(Expression.Between(filter.NomeCampo, filter.GetLowerValue(), filter.GetHigherValue()));
                            }
                            break;

                        case SearchType.BeginWith:
                            {
                                criterio = criterio.Add(Expression.Like(filter.NomeCampo, filter.GetBeginWithValue().ToString(), MatchMode.Start).IgnoreCase());
                            }
                            break;

                        case SearchType.Contains:
                            {
                                criterio = criterio.Add(Expression.Like(filter.NomeCampo, filter.GetContains().ToString(), MatchMode.Anywhere).IgnoreCase());
                            }
                            break;

                        case SearchType.Higher:
                            {
                                if (filter.GetHigherValue().GetType() == typeof(DateTime))
                                    criterio = criterio.Add(Expression.Gt(filter.NomeCampo, filter.GetHigherValue()));
                                //else if (IntService.IsNumerico(filter.GetHigherValue().GetType()))
                                //    criterio = criterio.Add(Expression.Gt(filter.NomeCampo, filter.GetHigherValue()));
                                else
                                    criterio = criterio.Add(Expression.Gt(filter.NomeCampo, filter.GetHigherValue()).IgnoreCase());
                            }
                            break;
                        case SearchType.HigherEqual:
                            {
                                if (filter.GetHigherEqualValue().GetType() == typeof(DateTime))
                                    criterio = criterio.Add(Expression.Ge(filter.NomeCampo, filter.GetHigherEqualValue()));
                                else if (IntService.IsNumerico(filter.GetHigherEqualValue().GetType()))
                                    criterio = criterio.Add(Expression.Ge(filter.NomeCampo, filter.GetHigherEqualValue()));
                                else
                                    criterio = criterio.Add(Expression.Ge(filter.NomeCampo, filter.GetHigherEqualValue()).IgnoreCase());
                            }
                            break;
                        case SearchType.Lower:
                            {
                                if (filter.GetLowerValue().GetType() == typeof(DateTime))
                                    criterio = criterio.Add(Expression.Lt(filter.NomeCampo, filter.GetLowerValue()));
                                else if (IntService.IsNumerico(filter.GetLowerValue().GetType()))
                                    criterio = criterio.Add(Expression.Lt(filter.NomeCampo, filter.GetLowerValue()));
                                else
                                    criterio = criterio.Add(Expression.Lt(filter.NomeCampo, filter.GetLowerValue()).IgnoreCase());
                            }
                            break;
                        case SearchType.LowerEqual:
                            {
                                if (filter.GetLowerEqualValue().GetType() == typeof(DateTime))
                                    criterio = criterio.Add(Expression.Le(filter.NomeCampo, filter.GetLowerEqualValue()));
                                else if (IntService.IsNumerico(filter.GetLowerEqualValue().GetType()))
                                    criterio = criterio.Add(Expression.Le(filter.NomeCampo, filter.GetLowerEqualValue()));
                                else
                                    criterio = criterio.Add(Expression.Le(filter.NomeCampo, filter.GetLowerEqualValue()).IgnoreCase());
                            }
                            break;

                    }
                }
                else
                {
                    String parent = filter.NomeCampo.Substring(0, filter.NomeCampo.IndexOf("."));

                    String[] _camadas = filter.NomeCampo.Split('.');
                    List<String> _camadasTmp = new List<string>();

                    for (int i = 1; i < _camadas.Length; i++)
                    {
                        _camadasTmp.Add(_camadas[i]);
                    }
                    String child = String.Join(".", _camadasTmp);

                    AddAssociatedCriteria<T>(criterio, parent, child, filter);
                }
            }

            foreach (FilterFetchMode fetch in filtros.GetFetchMode())
            {
                criterio.Fetch(SelectMode.Fetch, fetch.FieldName);
            }

            foreach (OrderBy order in filtros.GetOrderList())
            {
                if (order.Direction == OrderByIdentity.Asc)
                {
                    criterio.AddOrder(Order.Asc(order.NomeCampo));
                }
                else
                {
                    criterio.AddOrder(Order.Desc(order.NomeCampo));
                }
            }

            if (filtros.GetMaxResults() > 0)
            {
                criterio.SetMaxResults(filtros.GetMaxResults());
            }

            if (filtros.GetFirstResult() > 0)
            {
                criterio.SetFirstResult(filtros.GetFirstResult());
            }
        }


        public void FilterToCriteria(Type tipo, ICriteria criterio, Filters filtros)
        {
            foreach (Filter filter in filtros.GetFilterList())
            {
                if (!filter.NomeCampo.Contains("."))
                {
                    CreateCriteriaByFilter(filter, criterio);
                }
                else
                {
                    String parent = filter.NomeCampo.Substring(0, filter.NomeCampo.IndexOf("."));

                    String child = filter.NomeCampo.Replace(parent + ".", null);

                    AddAssociatedCriteria(tipo, ref criterio, parent, child, filter);
                }
            }

            foreach (FilterFetchMode fetch in filtros.GetFetchMode())
            {
                criterio.Fetch(SelectMode.Fetch, fetch.FieldName);
            }

            foreach (OrderBy order in filtros.GetOrderList())
            {
                if (order.Direction == OrderByIdentity.Asc)
                {
                    criterio.AddOrder(Order.Asc(order.NomeCampo));
                }
                else
                {
                    criterio.AddOrder(Order.Desc(order.NomeCampo));
                }
            }

            if (filtros.GetMaxResults() > 0)
            {
                criterio.SetMaxResults(filtros.GetMaxResults());
            }

            if (filtros.GetFirstResult() > 0)
            {
                criterio.SetFirstResult(filtros.GetFirstResult());
            }

        }


        private void CreateCriteriaByFilter(Filter filter, ICriteria criterio)
        {
            switch (filter.Expressao)
            {
                case SearchType.Equal:
                    {
                        if (filter.GetUniqueValue() == null)
                            criterio = criterio.Add(Expression.IsNull(filter.NomeCampo));
                        else if (filter.GetUniqueValue().GetType() == typeof(String))
                            criterio = criterio.Add(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue()).IgnoreCase());
                        else
                            criterio = criterio.Add(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue()));
                    }
                    break;

                case SearchType.NotEqual:
                    {
                        if (filter.GetUniqueValue() == null)
                            criterio = criterio.Add(Expression.IsNotNull(filter.NomeCampo));
                        else if (filter.GetUniqueValue().GetType() == typeof(String))
                            criterio = criterio.Add(Expression.Not(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue()).IgnoreCase()));
                        else
                            criterio = criterio.Add(Expression.Not(Expression.Eq(filter.NomeCampo, filter.GetUniqueValue())));
                    }
                    break;

                case SearchType.BeginWith:
                    {
                        criterio = criterio.Add(Expression.Like(filter.NomeCampo, filter.GetBeginWithValue().ToString(), MatchMode.Start).IgnoreCase());
                    }
                    break;

                case SearchType.Between:
                    {
                        criterio = criterio.Add(Expression.Between(filter.NomeCampo, filter.GetLowerValue(), filter.GetHigherValue()));
                    }
                    break;
                case SearchType.Contains:
                    {
                        criterio = criterio.Add(Expression.Like(filter.NomeCampo, filter.GetContains().ToString(), MatchMode.Anywhere).IgnoreCase());
                    }
                    break;

                case SearchType.Higher:
                    {
                        criterio = criterio.Add(Expression.Gt(filter.NomeCampo, filter.GetHigherValue()));
                    }
                    break;
                case SearchType.HigherEqual:
                    {

                        criterio = criterio.Add(Expression.Ge(filter.NomeCampo, filter.GetHigherEqualValue()));
                    }
                    break;
                case SearchType.Lower:
                    {
                        criterio = criterio.Add(Expression.Lt(filter.NomeCampo, filter.GetLowerValue()));
                    }
                    break;
                case SearchType.LowerEqual:
                    {
                        criterio = criterio.Add(Expression.Le(filter.NomeCampo, filter.GetLowerEqualValue()));
                    }
                    break;
            }
        }





        public void AddCriteriaExpression(ICriteria criteria, String fieldName, object value, SearchType searchType)
        {
            if (searchType == SearchType.Equal)
            {
                if (value == null)
                    criteria = criteria.Add(Expression.IsNull(fieldName));
                else if (value.GetType() == typeof(String))
                    criteria = criteria.Add(Expression.Eq(fieldName, value).IgnoreCase());
                else
                    criteria = criteria.Add(Expression.Eq(fieldName, value));
            }
            else if (searchType == SearchType.Contains)
            {
                criteria = criteria.Add(Expression.InsensitiveLike(fieldName, value.ToString(), MatchMode.Anywhere));
            }
            else if (searchType == SearchType.BeginWith)
            {
                criteria = criteria.Add(Expression.InsensitiveLike(fieldName, value.ToString(), MatchMode.Start));
            }
        }

        public int GetCountByToken<T>(string pToken)
        {
            String sSql = String.Format("SELECT COUNT(OBJ) FROM {0} OBJ WHERE OBJ.Token= :TOKEN", typeof(T).Name);

            Int32 retorno = Convert.ToInt32(GetSession().CreateQuery(sSql).SetString("TOKEN", pToken).UniqueResult());

            return retorno;
        }

        public bool ExecuteDDL(String pScript)
        {
            GetSession().CreateSQLQuery(pScript).ExecuteUpdate();

            return true;
        }


        public byte[] ConvertFileToArrayByte(String fullFilePath)
        {
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(fullFilePath);
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));

                return bytes;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
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


    }
}
