using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinapse.Model.AspNetCore.core
{
    public interface IGenericRepository<T, K>
        where T : class
    {

        Task<M> GetMax<M>(System.Linq.Expressions.Expression<Func<T, object>> expression);
        Task<ICollection<T>> FindAll();
        Task<ICollection<T>> FindAll(int skip, int take);
        Task<ICollection<T>> FindAllByWhere(global::System.Linq.Expressions.Expression<Func<T, bool>> where);
        Task<ICollection<T>> FindAllByWhere(global::System.Linq.Expressions.Expression<Func<T, bool>> where, int skip, int take);
        Task<T> FindById(K key);
        Task<T> FindByWhere(global::System.Linq.Expressions.Expression<Func<T, bool>> where);
        Task<object> GetByIdStateless(Type tipo, long oid);
        Task<int> GetCount();
        Task<IList<object>> GetList(Type tipo, int MaxResults);
        Task<IList<T>> GetListByField(string fieldName, object value);
        Task<IList<T>> GetListByField(string fieldName, object value, SearchType searchType);
        Task<IList<T>> GetListByField(string fieldName, object value, SearchType searchType, int MaxResult);
        Task<IList<object>> GetListByField(Type tipo, string fieldName, object value, SearchType searchType);
        Task<IList<T>> GetListByFiltro(Filters filtros);
        Task<IList<object>> GetListByFiltro(Type tipo, Filters filtros);
        Task<IList<T>> GetListByFiltroEagger(Filters filtros, string EaggerProperty);
        Task<IList<T>> GetListByFiltroPage(Filters filtros, int pageIndex, int pageSize);
        Task<IList<object>> GetListByFiltroPage(Type tipo, Filters filtros, int pageIndex, int pageSize);
        Task<T> GetObjectByField(string fieldName, object value);
        Task<object> GetObjectByField(Type tipo, string fieldName, object value);
        Task<T> GetObjectByFiltro(Filters filtros);
        Task<T> GetObjectByFiltro(Filters filtros, global::NHibernate.SelectMode mode, string EaggerProperty);
        
        global::NHibernate.IStatelessSession GetStatelessSession();
        Task<bool> Insert(global::System.Collections.Generic.ICollection<T> objs);
        Task<bool> Insert(T obj);
        Task<bool> InsertBatch(T obj);
        Task<bool> InsertOrUpdate(T obj);
        Task<bool> Remove(T obj);
        Task<bool> RemoveAll();
        Task<bool> Update(T obj);
        Task<bool> UpdateBatch(T obj);
        Task<Boolean> InsertLog(Object obj);
        Task<ICollection<T>> FindAllLog(int skip, int take);

        Task<ICollection<T>> FindAllLogByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where);

        Task<ICollection<T>> FindAllLogByWhere(System.Linq.Expressions.Expression<Func<T, bool>> where, int skip, int take);
    }
}
