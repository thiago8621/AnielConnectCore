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

namespace Sinapse.DAO.repository
{
    public class ConexaoStateless<T> : IDisposable where T : class
    {
        private IStatelessSession _session;

        public ConexaoStateless(IStatelessSession session)
        {
            _session = session;
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

        public async Task<IList<T>> select(Expression<Func<T, bool>> expression)
        {
            return await _session.QueryOver<T>().Where(expression).ListAsync();
        }

        public async Task<T> selectFirst()
        {
            IList<T> lista = await _session.QueryOver<T>().ListAsync();

            return lista.FirstOrDefault();
        }

        public async Task<T> selectFirst(Expression<Func<T, bool>> expression)
        {
            IList<T> lista = await _session.QueryOver<T>().Where(expression).ListAsync();

            return lista.FirstOrDefault();
        }

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
        }

        public async Task Save(T obj)
        {
            await _session.InsertAsync(obj);
        }

        public async Task Save(List<T> obj)
        {
            foreach (T salvar in obj)
            {
                await _session.InsertAsync(salvar);
            }
        }

        public async Task RemoveAll()
        {
            string delete = string.Format("delete {0}", typeof(T));
            await _session.CreateSQLQuery(delete).ExecuteUpdateAsync();
        }

        public async Task Remove(T obj)
        {
            await _session.DeleteAsync(obj);
        }

        public async Task Remove(IList<T> obj)
        {
            foreach (T del in obj)
            {
                await _session.DeleteAsync(del);
            }
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