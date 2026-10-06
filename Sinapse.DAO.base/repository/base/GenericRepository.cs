using Sinapse.Model;
using Sinapse.Model.nhibernate;
using Sinapse.Util;
using NHibernate;
using NHibernate.Context;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sinapse.Model.core;
using Sinapse.DAO.service;

namespace Sinapse.DAO.repository
{
    public abstract class GenericRepository<T, K> : IGenericRepository<T, K>, IDisposable
        where T : class
    {
        protected ISession session;
        protected ISession sessionLog;
        protected IStatelessSession statelesssession;

        private GenericRepository()
        {

        }

        public GenericRepository(ISession Session, ISession SessionLog)
        {
            session = Session;
            sessionLog = SessionLog;
        }

        public IStatelessSession GetStatelessSession()
        {
            if (statelesssession == null)
            {
                statelesssession = HibernateUtil.GetSessionFactory().Result.OpenStatelessSession();
            }
            return statelesssession;
        }


        public async Task<M> GetMax<M>(System.Linq.Expressions.Expression<Func<T, object>> expression)
        {
            try
            {
                return (await session.QueryOver<T>()
                              .Select(
                                    Projections
                                       .ProjectionList()
                                       .Add(Projections.Max<T>(expression)))
                              .ListAsync<M>()).First();
            }
            catch
            {
                return default(M);
            }
        }

        public async Task<bool> InsertBatch(T obj)
        {
            IStatelessSession session = GetStatelessSession();

            try
            {
                await session.InsertAsync(obj);
            }
            catch (Exception e)
            {
                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message, e, obj);
            }

            return true;
        }

        public async Task<bool> UpdateBatch(T obj)
        {
            IStatelessSession session = GetStatelessSession();

            try
            {
                await session.UpdateAsync(obj);
            }
            catch (Exception e)
            {
                throw new PersistenceException("Ocorreu um erro ao realizar uma operação no banco de dados." + e.Message,
                    e, obj);
            }

            return true;
        }

        

        public async Task<object> GetByIdStateless(Type tipo, long oid)
        {
            object obj = null;
            IStatelessSession session = GetStatelessSession();

            try
            {
                obj = await session.GetAsync(tipo.FullName, oid);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return obj;
        }

        public async Task<T> GetObjectByField(String fieldName, Object value)
        {
            T objRetorno = default(T);

            ICriteria criterio = null;

            if (value == null)
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.IsNull(fieldName));
            else
            {
                criterio = session.CreateCriteria(typeof(T)).Add(Expression.Eq(fieldName, value).IgnoreCase());
            }

            try
            {

                IList<T> listaRetorno = await criterio.ListAsync<T>();

                if (listaRetorno.Count > 0)
                    objRetorno = listaRetorno[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return objRetorno;

        }

        public async Task<IList<T>> GetListByField(String fieldName, Object value)
        {
            if (session == null)
                throw new Exception("Sessão nula");

            IList<T> listQry = null;
            ICriteria criteria = session.CreateCriteria(typeof(T));

            if (criteria == null)
                throw new Exception("Criteria nula");

            try
            {
                if (value != null)
                    criteria = criteria.Add(Expression.Eq(fieldName, value));
                else
                    criteria = criteria.Add(Expression.IsNull(fieldName));

                listQry = await criteria.ListAsync<T>();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listQry;
        }

        public async  Task<IList<T>> GetListByField(String fieldName, Object value, SearchType searchType)
        {
            IList<T> listQry = null;

            ICriteria criteria = session.CreateCriteria(typeof(T));

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

                listQry = await criteria.ListAsync<T>();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listQry;
        }

        public async Task<IList<T>> GetListByField(String fieldName, Object value, SearchType searchType, int MaxResult)
        {
            IList<T> listQry = null;

            ICriteria criteria = session.CreateCriteria(typeof(T));

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

                listQry = await criteria.ListAsync<T>();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listQry;
        }

        public async Task<IList<T>> GetListByFiltroPage(Filters filtros, Int32 pageIndex, Int32 pageSize)
        {
            IList<T> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(typeof(T));
            try
            {

                FilterToCriteria(criterio, filtros);

                lstRetorno = await criterio.SetFirstResult(pageIndex - 1 * pageSize).SetMaxResults(pageSize).ListAsync<T>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstRetorno;
        }


        public async Task<IList<Object>> GetListByFiltroPage(Type tipo, Filters filtros, Int32 pageIndex, Int32 pageSize)
        {
            IList<Object> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(tipo);

            try
            {
                FilterToCriteria(tipo, criterio, filtros);

                lstRetorno = await criterio.SetFirstResult((pageIndex - 1) * pageSize).SetMaxResults(pageSize).ListAsync<Object>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstRetorno;
        }

        public async Task<IList<T>> GetListByFiltroEagger(Filters filtros, String EaggerProperty)
        {
            return await GetListByFiltro(filtros, SelectMode.Undefined, EaggerProperty);
        }

        public async Task<IList<T>> GetListByFiltro(Filters filtros)
        {
            return await GetListByFiltro(filtros, SelectMode.Undefined, "-");
        }

        public async Task<IList<Object>> GetListByFiltro(Type tipo, Filters filtros)
        {
            return await GetListByFiltro(tipo, filtros, SelectMode.Undefined, "-");
        }

        public async Task<T> GetObjectByFiltro(Filters filtros)
        {
            return await GetObjectByFiltro(filtros, SelectMode.Undefined, "-");
        }


        public async Task<IList<object>> GetListByField(Type tipo, String fieldName, Object value, SearchType searchType)
        {
            IList<object> listQry = null;

            ICriteria criteria = session.CreateCriteria(tipo);

            try
            {
                AddCriteriaExpression(criteria, fieldName, value, searchType);

                listQry = await criteria.ListAsync<object>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listQry;
        }

        public static void AddCriteriaExpression(ICriteria criteria, String fieldName, object value, SearchType searchType)
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

        public async Task<object> GetObjectByField(Type tipo, String fieldName, Object value)
        {
            object retorno = null;

            ICriteria criteria = session.CreateCriteria(tipo);

            try
            {
                AddCriteriaExpression(criteria, fieldName, value, SearchType.Equal);

                retorno = await criteria.UniqueResultAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retorno;
        }

        public async Task<IList<object>> GetList(Type tipo, int MaxResults)
        {
            IList<object> listQry = null;

            ICriteria criteria = session.CreateCriteria(tipo).SetMaxResults(MaxResults);

            try
            {
                listQry = await criteria.ListAsync<object>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return listQry;
        }

        private async Task<IList<T>> GetListByFiltro(Filters filtros, SelectMode mode, String EaggerProperty)
        {
            IList<T> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(typeof(T));

            try
            {
                FilterToCriteria(criterio, filtros);

                if (mode != SelectMode.Undefined)
                    lstRetorno = await criterio.Fetch(SelectMode.Fetch, EaggerProperty).ListAsync<T>();
                else
                    lstRetorno = await criterio.ListAsync<T>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstRetorno;
        }



        private async Task<IList<Object>> GetListByFiltro(Type tipo, Filters filtros, SelectMode mode, String EaggerProperty)
        {
            IList<Object> lstRetorno = null;

            ICriteria criterio = session.CreateCriteria(tipo);

            try
            {
                FilterToCriteria(tipo, criterio, filtros);

                if (mode != SelectMode.Undefined)
                    lstRetorno = await criterio.Fetch(SelectMode.Fetch, EaggerProperty).ListAsync<Object>();
                else
                    lstRetorno = await criterio.ListAsync<Object>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lstRetorno;
        }

        public async Task<T> GetObjectByFiltro(Filters filtros, SelectMode mode, String EaggerProperty)
        {
            T retorno = default(T);

            ICriteria criterio = session.CreateCriteria(typeof(T));

            try
            {
                FilterToCriteria(criterio, filtros);

                if (mode != SelectMode.Undefined)
                {
                    retorno = await criterio.Fetch(mode, EaggerProperty).UniqueResultAsync<T>();
                }
                else
                {
                    IList<T> lista = await criterio.ListAsync<T>();
                    if (lista.Count == 0)
                        retorno = default(T);
                    else
                        retorno = lista[0];
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retorno;
        }

        public static void FilterToCriteria(ICriteria criterio, Filters filtros)
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
                                else if (IntService.IsNumerico(filter.GetHigherValue().GetType()))
                                    criterio = criterio.Add(Expression.Gt(filter.NomeCampo, filter.GetHigherValue()));
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

                    AddAssociatedCriteria(criterio, parent, child, filter);
                }
            }

            foreach (FilterFetchMode fetch in filtros.GetFetchMode())
            {
                criterio.Fetch(fetch.FetchModeField, fetch.FieldName);
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
                criterio.Fetch(fetch.FetchModeField, fetch.FieldName);
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

        private static void AddAssociatedCriteria(ICriteria criterioBase, String ParentName, String FieldName, Filter filtro)
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

        private static void AddAssociatedCriteria(Type tipo, ref ICriteria criterioBase, String ParentName, String FieldName, Filter filtro)
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

                                criterioAssociado.Fetch(SelectMode.Fetch, pp.PropertyType.Name);

                                AddAssociatedCriteria(p.PropertyType, ref criterioAssociado, parent, child, filtro);
                            }
                        }
                    }

                }
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

        public async Task<Boolean> Insert(T obj) 
        {
            try
            {
                await session.SaveAsync(obj);
            }
            catch
            {
                throw;
            }

            return true;
        }

        public async Task<Boolean> Insert(ICollection<T> objs)
        {
            try
            {
                GetStatelessSession().SetBatchSize(200);

                int i = 0;
                foreach (T obj in objs)
                {
                    await GetStatelessSession().InsertAsync(obj);

                    if (i == 200)
                    {
                        statelesssession = HibernateUtil.GetSessionFactory().Result.OpenStatelessSession();
                        i = 0;
                    }

                    i++;
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Boolean> RemoveAll()
        {
            try
            {
                statelesssession = GetStatelessSession();
                await statelesssession.CreateQuery(string.Format("delete from {0}", typeof(T).Name)).ExecuteUpdateAsync();
            }
            catch
            {
                throw;
            }

            return true;
        }

        public async Task<Boolean> Remove(T obj)
        {
            try
            {
                await session.DeleteAsync(obj);
            }
            catch
            {
                throw;
            }

            return true;
        }

        /// <summary>
        /// Remove gerando log
        /// </summary>
        /// <param name="obj">Objeto</param>
        /// <param name="paramWebApi">Parametros de tela</param>
        /// <returns></returns>
        public async Task<bool> Remove(T obj, IParamWebApi paramWebApi)
        {
            try
            {
                return await Remove(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> Update(T obj)
        {
            try
            {
                await session.UpdateAsync(obj);
            }
            catch
            {
                throw;
            }

            return true;
        }

        public async Task<Boolean> InsertOrUpdate(T obj)
        {
            try
            {
                await session.SaveOrUpdateAsync(obj);
            }
            catch
            {
                throw;
            }

            return true;
        }

        /// <summary>
        /// Insere ou Altera com Log
        /// </summary>
        /// <param name="obj">Objeto</param>
        /// <param name="paramWebApi">Parametros de tela</param>
        /// <returns></returns>
        public async Task<bool> InsertOrUpdate(T obj, IParamWebApi paramWebApi)
        {
            try
            {
                Boolean retorno = false;

                FillUsuarioCriacaoAlteracao(obj, paramWebApi);

                Boolean IsIModel = (obj.GetType().GetInterface(typeof(IModel).ToString()) != null);

                //Define o tipo de operação para o log
                retorno = await InsertOrUpdate(obj);
                return retorno;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillUsuarioCriacaoAlteracao(T obj, IParamWebApi paramWebApi)
        {
            if (obj.GetType().GetInterface(typeof(IModel).ToString()) != null)
            {
                if (((IModel)obj).Id == 0)
                {
                    ((IModel)obj).UsuarioCriacao = paramWebApi.Usuario;
                    ((IModel)obj).DataHoraCriacao = DateTime.Now;
                }
                else
                {
                    ((IModel)obj).UsuarioAlteracao = paramWebApi.Usuario;
                    ((IModel)obj).DataHoraAlteracao = DateTime.Now;
                }

                ((IModel)obj).Token = paramWebApi.Token;
            }
        }

        public async Task<ICollection<T>> FindAll()
        {
            try
            {
                return await session.CreateCriteria(typeof(T)).ListAsync<T>();
            }
            catch
            {
                throw;
            }

        }

        public async Task<ICollection<T>> FindAll(int skip, int take)
        {
            try
            {
                return await session.CreateCriteria(typeof(T)).SetFirstResult(skip).SetMaxResults(take).ListAsync<T>();
            }
            catch
            {
                throw;
            }
        }

        //[Obsolete("Method obsoleto use Conexao.selectFirst.")]
        public async Task<T> FindById(K key)
        {
            try
            {
                return (T)await session.GetAsync(typeof(T), key);
            }
            catch
            {
                throw;
            }

        }

        public async Task<int> GetCount()
        {
            try
            {
                return await session.QueryOver<T>().Select(Projections.RowCount()).FutureValue<Int32>().GetValueAsync();
            }
            catch
            {
                throw;
            }

        }

        public async Task<ICollection<T>> FindAllByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where)
        {
            try
            {
                return await session.QueryOver<T>().Where(where).ListAsync<T>();
            }
            catch
            {
                throw;
            }

        }

        public async Task<ICollection<T>> FindAllByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where, int skip, int take)
        {
            try
            {
                return await session.QueryOver<T>().Where(where).Skip(skip).Take(take).ListAsync<T>();
            }
            catch
            {
                throw;
            }

        }

        public async Task<T> FindByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where)
        {
            try
            {
                return await session.QueryOver<T>().Where(where).SingleOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        #region Log
        public async Task<Boolean> InsertLog(Object obj)
        {
            try
            {
                await sessionLog.SaveAsync(obj);
            }
            catch
            {
                throw;
            }

            return true;
        }

        public async Task<ICollection<T>> FindAllLog(int skip, int take)
        {
            try
            {
                return await sessionLog.CreateCriteria(typeof(T)).SetFirstResult(skip).SetMaxResults(take).ListAsync<T>();
            }
            catch
            {
                throw;
            }
        }

        public async Task<ICollection<T>> FindAllLogByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where)
        {
            try
            {
                return await sessionLog.QueryOver<T>().Where(where).ListAsync<T>();
            }
            catch
            {
                throw;
            }

        }

        public async Task<ICollection<T>> FindAllLogByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where, int skip, int take)
        {
            try
            {
                return await sessionLog.QueryOver<T>().Where(where).Skip(skip).Take(take).ListAsync<T>();
            }
            catch
            {
                throw;
            }

        }

        #endregion

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
