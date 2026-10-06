using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinapse.DAO.AspNetCore.repository
{
    /// <summary>
    /// Query com select feito pelo programador
    /// </summary>
    public class Query : IDisposable
    {
        private string sql;
        private Dictionary<string, object> parametros;
        private ISession consulta;
        private bool transaction = false;
        private ISession _session;

        public Query(ISession session)
        {
            _session = session;
        }

        public void startTransaction()
        {
            transaction = true;
        }

        public void commitTransaction()
        {
            if (consulta != null)
                consulta.Transaction.Commit();
        }

        public void rollbackTransaction()
        {
            if (consulta != null)
                consulta.Transaction.Rollback();
        }

        public void Select(string consulta)
        {
            this.sql = consulta;
        }

        public void setParametro(string nome, object valor)
        {
            if (parametros == null) parametros = new Dictionary<string, object>();
            if (!parametros.ContainsKey(nome))
            {
                if ((valor == null) || ((valor is string) && (string.IsNullOrEmpty((string)valor)))) parametros.Add(nome, null);
                else parametros.Add(nome, valor);
            }
        }

        public async Task<List<Hashtable>> Values(string consulta)
        {
            this.Select(consulta);
            return await this.Values();
        }

        private IQuery mount()
        {
            if (consulta == null)
            {
                consulta = _session;
                if (transaction) consulta.BeginTransaction();
            }

            var sqlQuery = consulta.CreateSQLQuery(this.sql);
            if (parametros != null)
            {
                foreach (string key in parametros.Keys)
                {
                    if (sqlQuery.NamedParameters.Where(p => p.Equals(key)).Count() > 0)
                    {
                        if (parametros[key] == null) sqlQuery.SetString(key, null);
                        else sqlQuery.SetParameter(key.Replace(":", ""), parametros[key]);
                    }
                }
            }

            return sqlQuery.SetResultTransformer(NHibernate.Transform.Transformers.AliasToEntityMap);
        }

        
        #region
        public async Task<List<Hashtable>> Values()
        {
            List<Hashtable> retono = new List<Hashtable>();

            var lista = await this.mount().ListAsync<Hashtable>();
            retono = new List<Hashtable>(lista);
            this.clear();
            return retono;
        }

        private IQuery mount(IResultTransformer transformer)
        {
            if (consulta == null)
            {
                consulta = _session;
                if (transaction) consulta.BeginTransaction();
            }

            var sqlQuery = consulta.CreateSQLQuery(this.sql);
            if (parametros != null)
            {
                foreach (string key in parametros.Keys)
                {
                    if (sqlQuery.NamedParameters.Where(p => p.Equals(key)).Count() > 0)
                    {
                        if (parametros[key] == null) sqlQuery.SetString(key, null);
                        else sqlQuery.SetParameter(key.Replace(":", ""), parametros[key]);
                    }
                }
            }

            //return sqlQuery.SetResultTransformer(NHibernate.Transform.Transformers.AliasToEntityMap);
            return sqlQuery.SetResultTransformer(transformer);
        }

        /// <summary>
        /// Metodo onde no select é necessário se ter os alias completamente iguais aos nomes de suas properties.
        /// Properties devem ser objetos simples
        /// </summary>
        /// <typeparam name="T">Classe que vai ser o retorno</typeparam>
        /// <returns></returns>
        public async Task<List<T>> Values<T>(string consulta) where T : class, new()
        {
            this.Select(consulta);
            List<T> retono = new List<T>();
            retono = (await this.mount(new Transform<T>()).ListAsync<T>()).ToList();
            this.clear();
            return retono;
        }

        private class Transform<T> : IResultTransformer where T : new()
        {
            public IList TransformList(IList collection)
            {
                return collection;
            }            

            public object TransformTuple(object[] tuple, string[] aliases)
            {
                T retorno = new T();
                foreach (var p in retorno.GetType().GetProperties())
                {
                    string nome = p.Name;

                    int index = Array.FindIndex(aliases, l => l.ToUpper() == nome.ToUpper());
                    var valor = tuple.ElementAtOrDefault(index);

                    if (valor == null) continue;
                    else if (p.PropertyType == typeof(Double)) valor = Convert.ToDouble(valor);
                    else if (p.PropertyType == typeof(Decimal)) valor = Convert.ToDecimal(valor);
                    else if (p.PropertyType == typeof(Int16)) valor = Convert.ToInt16(valor);
                    else if (p.PropertyType == typeof(Int32)) valor = Convert.ToInt32(valor);
                    else if (p.PropertyType == typeof(Int64)) valor = Convert.ToInt64(valor);
                    else if ((p.PropertyType == typeof(Boolean)) && (valor.GetType() == typeof(String)))
                    {
                        valor = new String[] { "S", "SIM", "T", "TRUE" }.Contains(valor.ToString().ToUpper());
                    }

                    p.SetValue(retorno, valor);
                }

                if ((retorno.GetType().GetProperties().Count() < 1) && (typeof(object) == retorno.GetType()))
                {
                    return tuple;
                }                

                return retorno;
            }
        }

        #endregion

        public void UpdateOrInsert()
        {
            this.mount().ExecuteUpdate();
            this.clear();
        }        

        private void clear()
        {
            this.sql = string.Empty;
            this.parametros = null;
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

            if ((transaction) && (consulta != null) && (consulta.Transaction != null) && (consulta.Transaction.IsActive))
                consulta.Transaction.Commit();
        }
        #endregion

    }
}
