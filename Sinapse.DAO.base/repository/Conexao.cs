using NHibernate;
using NHibernate.Cfg;
using System;
using System.Reflection;
using NHibernate.Criterion;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using Sinapse.Model.core;

namespace Sinapse.DAO.repository
{
    public class Conexao<T> : IDisposable, IConexao<T> where T : class
    {
        private ISession _session;

        public Conexao(ISession session)
        {
            _session = session;
        }

        public ISession GetSession()
        {
            return _session;
        }

        public async Task<IList<T>> select()
        {
            return await _session.QueryOver<T>().ListAsync();
        }

        public IQueryOver<T, T> queryover()
        {
            var retono = _session.QueryOver<T>();
            return retono;
        }

        public IQueryOver<T, T> queryover(Expression<Func<T>> expression)
        {
            var retono = _session.QueryOver<T>(expression);
            return retono;
        }

        public async Task<IList<T>> selectCriteria(params ICriterion[] criterion)
        {
            var criteria = _session.CreateCriteria<T>();
            foreach (ICriterion cri in criterion)
            {
                criteria.Add(cri);
            }

            return await criteria.ListAsync<T>();
        }

        public object Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<T>> select(Expression<Func<T, bool>> expression)
        {
            return await _session.QueryOver<T>().Where(expression).ListAsync();
        }
        
        public async Task<IList<T>> selectOrderBy(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy)
        {
            return await _session.QueryOver<T>().Where(expression).OrderBy(orderBy).Desc.ListAsync();
        }

        public async Task<T> selectFirst()
        {
            IList<T> lista = await _session.QueryOver<T>().ListAsync();

            return lista.FirstOrDefault();
        }

        public async Task<T> selectLast(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy)
        {
            return (await _session.QueryOver<T>()
                .Where(expression)
                .OrderBy(orderBy).Desc
                .Take(1)
                .ListAsync())
                .FirstOrDefault();
        }

        public async Task<T> selectFirst(Expression<Func<T, bool>> expression)
        {
            //IList<T> lista = ;

            return (await _session.QueryOver<T>().Where(expression).Take(1).ListAsync()).FirstOrDefault();
        }

        #region IQueryable

        public IQueryable<T> QueryableSelect()
        {
            return this._session.Query<T>();
        }

        #endregion

        public async Task<IList<T>> selectSql(string consulta)
        {
            return await _session.CreateSQLQuery(consulta).ListAsync<T>();
        }

        public async Task<Int64> Max(Expression<Func<T, object>> expression, Expression<Func<T, bool>> where = null)
        {
            var tmp = _session.QueryOver<T>().Select(Projections.Max<T>(expression));

            if (where != null)
            {
                tmp = tmp.Where(where);
            }

            object retorno = await tmp.SingleOrDefaultAsync<object>();

            return retorno == null ? 0 : Convert.ToInt64(retorno);
        }

        public async Task<DateTime?> MaxDate(Expression<Func<T, object>> expression, Expression<Func<T, bool>> where = null)
        {
            var tmp = _session.QueryOver<T>().Select(Projections.Max<T>(expression));

            if (where != null)
            {
                tmp = tmp.Where(where);
            }

            object retorno = await tmp.SingleOrDefaultAsync<object>();

            return retorno == null ? (DateTime?)null : Convert.ToDateTime(retorno);
        }

        public async Task<Double> Sum(Expression<Func<T, object>> expression, Expression<Func<T, bool>> where = null)
        {
            var tmp = _session.QueryOver<T>().Select(Projections.Sum<T>(expression));

            if (where != null)
            {
                tmp = tmp.Where(where);
            }

            object retorno = await tmp.SingleOrDefaultAsync<object>();

            return retorno == null ? 0 : Convert.ToDouble(retorno);
        }

        public async Task<List<TResult>> SumGroupBy<TResult>(Expression<Func<T, object>> groupBy, Expression<Func<T, object>> sum, Expression<Func<T, bool>> where = null)
        {
            var query = _session.QueryOver<T>();

            if (where != null)
                query = query.Where(where);

            var result = await query
                .SelectList(list => list
                    .SelectGroup(groupBy)
                    .Select(Projections.Sum(sum)))
                .ListAsync<object[]>();

            var retorno = new List<TResult>();

            foreach (var item in result)
            {
                var obj = Activator.CreateInstance(typeof(TResult));

                var props = typeof(TResult).GetProperties();

                props[0].SetValue(obj, item[0] == null ? null : Convert.ChangeType(item[0], props[0].PropertyType));
                props[1].SetValue(obj, item[1] == null ? null : Convert.ChangeType(item[1], props[1].PropertyType));

                retorno.Add((TResult)obj);
            }

            return retorno;
        }

        public async Task<Int32> Count(Expression<Func<T, bool>> where = null)
        {
            var tmp = _session.QueryOver<T>().Select(Projections.RowCount());

            if (where != null)
            {
                tmp = tmp.Where(where);
            }

            object retorno = await tmp.SingleOrDefaultAsync<object>();

            return retorno == null ? 0 : Convert.ToInt32(retorno);
        }

        public async Task<Int64> MaxString(String Campo, String Tabela, String Where = "")
        {
            var sql = String.Empty;

            if (_session.Connection.GetType().Name == "FbConnection")
                sql = $"SELECT COALESCE(MAX((SELECT RETORNO FROM extract_integer({Campo}))),0) AS CAMPO FROM {Tabela}";
            else if (_session.Connection.GetType().Name == "MySqlConnection")
                sql = $"SELECT IFNULL(MAX(CAST({Campo} AS unsigned integer)),0) AS CAMPO FROM {Tabela}";
            else
                sql = $"SELECT IFNULL(MAX(CAST({Campo} AS INT)),0) AS CAMPO FROM {Tabela}";

            if (!String.IsNullOrEmpty(Where))
                sql += $" WHERE {Where}";

            var retorno = await _session.CreateSQLQuery(sql).ListAsync();

            if (retorno.Count > 0)
            {
                return Convert.ToInt64(retorno[0]);
            }

            return 0;
        }

        public async Task<Int64> GetSequence(String Sequence)
        {
            var sql = String.Empty;

            if (_session.Connection.GetType().Name == "FbConnection")
                sql = $"SELECT gen_id({Sequence},1) FROM RDB$DATABASE";
            /*else if (_session.Connection.GetType().Name == "MySqlConnection")
                sql = $"SELECT IFNULL(MAX(CAST({Campo} AS unsigned integer)),0) AS CAMPO FROM {Tabela}";
            else
                sql = $"SELECT IFNULL(MAX(CAST({Campo} AS INT)),0) AS CAMPO FROM {Tabela}";

            if (!String.IsNullOrEmpty(Where))
                sql += $" WHERE {Where}"; */

            var retorno = await _session.CreateSQLQuery(sql).ListAsync();

            if (retorno.Count > 0)
            {
                return Convert.ToInt64(retorno[0]);
            }

            return 0;
        }

        public async Task Update(T obj)
        {
            await _session.UpdateAsync(obj);
            await _session.FlushAsync();
        }

        public async Task Update(List<T> obj)
        {            
            foreach (T update in obj)
            {
                await _session.UpdateAsync(update);
            }
            await _session.FlushAsync();
        }

        public async Task SaveOrUpdate(T obj)
        {
            await _session.SaveOrUpdateAsync(obj);
            await _session.FlushAsync();
        }

        public async Task Update(T obj, Int64? id)
        {
            await _session.UpdateAsync(obj, id);
            await _session.FlushAsync();
        }

        public async Task SaveOrUpdate(IEnumerable<T> listGravacao)
        {
            foreach (T obj in listGravacao)
            {
                await _session.SaveOrUpdateAsync(obj);
            }
            await _session.FlushAsync();
        }

        public async Task Merge(T obj)
        {
            await _session.MergeAsync(obj);
            await _session.FlushAsync();
        }

        public async Task Merge(IEnumerable<T> listGravacao)
        {
            foreach (T obj in listGravacao)
            {
                await _session.MergeAsync(obj);
            }

            await _session.FlushAsync();
        }

        public async Task Save(T obj)
        {
            await _session.SaveAsync(obj);
            await _session.FlushAsync();
        }

        public async Task Save(List<T> obj)
        {
            foreach (T salvar in obj)
            {
                await _session.SaveAsync(salvar);
            }
            await _session.FlushAsync();
        }

        public async Task RemoveAll()
        {
            string delete = string.Format("delete {0}", typeof(T));
            await _session.CreateSQLQuery(delete).ExecuteUpdateAsync();
            await _session.FlushAsync();
        }

        public async Task Remove(T obj)
        {
            await _session.DeleteAsync(obj);
            await _session.FlushAsync();
        }

        public async Task Remove(IEnumerable<T> obj)
        {
            foreach (T del in obj)
            {
                await _session.DeleteAsync(del);
            }
            await _session.FlushAsync();
        }

        public async Task<List<T>> query(String tableName, String[] campos, String where)
        {
            Dictionary<String, String> dic = new Dictionary<string, string>();
            Dictionary<String, Int32> dicIndex = new Dictionary<string, int>();

            Int32 idx = 0;
            foreach (PropertyInfo prop in Activator.CreateInstance(typeof(T)).GetType().GetProperties())
            {
                foreach (String campo in campos)
                {
                    if (prop.Name.ToUpper().Equals(campo.ToUpper()))
                    {
                        dic.Add(campo, prop.Name);
                        dicIndex.Add(campo, idx);
                        idx++;
                    }
                }
            }

            String sql = String.Format("SELECT {0} FROM {1} WHERE {2}", string.Join(",", campos), tableName, where);
            List<T> retorno = new List<T>();

            foreach (Object[] obj in await _session.CreateSQLQuery(sql).ListAsync())
            {
                T tObj = (T)Activator.CreateInstance(typeof(T));

                foreach (String campo in campos)
                {
                    try
                    {
                        tObj.GetType().GetProperty(dic[campo]).SetValue(tObj, obj[dicIndex[campo]]);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("System.Int32"))
                            tObj.GetType().GetProperty(dic[campo]).SetValue(tObj, int.Parse(obj[dicIndex[campo]].ToString()));
                        else if (e.Message.Contains("System.Int64"))
                            tObj.GetType().GetProperty(dic[campo]).SetValue(tObj, long.Parse(obj[dicIndex[campo]].ToString()));
                        else if (e.Message.Contains("System.Double"))
                            tObj.GetType().GetProperty(dic[campo]).SetValue(tObj, Double.Parse(obj[dicIndex[campo]].ToString()));
                    }
                }

                retorno.Add(tObj);
            }

            return retorno;
        }

        #region IDisposable Support
        private Boolean disposedValue;// To detect redundant calls

        // IDisposable
        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }
            this.disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        #endregion
    }

    //public static class Conexao
    //{
    //    private static object padlock = new Object();
    //    private static Configuration configuracao;
    //    private static ISessionFactory factory;


    //    public static ISession GetSession()
    //    {
    //        try
    //        {
    //            ISession retorno = NHibernateHelper.GetInstance().GetSession();
    //            retorno.FlushMode = FlushMode.Always;
    //            return retorno;
    //        }
    //        catch (Exception ex)
    //        {
    //            if (!ex.Message.Equals("O thread estava sendo anulado."))
    //                throw new Exception("Erro para recuperar sessão " + ex.Message);
    //            else
    //                return null;
    //        }
    //    }

    //    public static void LoadConfiguration()
    //    {
    //        try
    //        {
    //            NHibernateProfiler.Initialize();

    //            configuracao = new Configuration();
    //            configuracao.Configure(String.Format(@"{0}\hibernate.cfg.xml", Directory.GetCurrentDirectory()));
    //            //configuracao.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
    //            try
    //            {
    //                configuracao.AddAssembly(typeof(BaseClass).GetTypeInfo().Assembly);
    //            }
    //            catch (Exception ex)
    //            {
    //                throw new Exception(String.Format("Erro ao carregar assemblys Business ({0}). {1}",
    //                    ex.Message, ex.InnerException == null ? String.Empty : ex.InnerException.Message), ex);
    //            }

    //        }
    //        catch (Exception e)
    //        {
    //            throw e;
    //        }
    //    }

    //    public static ISessionFactory GetSessionFactory()
    //    {
    //        lock (padlock)
    //        {

    //            if (factory == null)
    //            {
    //                LoadConfiguration();

    //                factory = configuracao.BuildSessionFactory();
    //            }

    //            return factory;
    //        }
    //    }

    //    public static void CloseSessionFactory()
    //    {
    //        if (factory != null)
    //        {
    //            factory.Close();
    //            factory.Dispose();
    //            factory = null;
    //        }
    //    }

    //    public static string getBDFoto()
    //    {
    //        return System.Configuration.ConfigurationManager.AppSettings["BD_FOTO"].ToString();
    //    }
    //}
}