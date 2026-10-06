using Sinapse.Util.AspNetCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.AspNetCore.repository
{
    public class CommandHelper
    {
        public static DbTransaction BeginTransaction(DbConnection conn)
        {
            DbTransaction transaction = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

            return transaction;
        }

        #region ExecuteReader

        public async static Task<DbDataReader> ExecuteReader(DbCommand com, StringBuilder pSql, DbConnection FBcon, DbTransaction trans)
        {
            return await ExecuteReader(com, pSql, new Dictionary<string, string>(), FBcon, trans);
        }

        public async static Task<DbDataReader> ExecuteReader(DbCommand com, StringBuilder pSql, Dictionary<string, string> parametros, DbConnection FBcon, DbTransaction trans)
        {
            if (FBcon == null)
                throw new Exception(String.Format("Coneção não está criada {0}", pSql));

            if (FBcon.State == System.Data.ConnectionState.Closed) FBcon.Open();

            if (com == null)
                throw new Exception(String.Format("Driver de conexão não criou connection {0}", pSql));

            try
            {
                com.CommandText = pSql.ToString();
                com.Transaction = trans;

                com.Parameters.Clear();
                foreach (KeyValuePair<string, string> par in parametros)
                {
                    var parameter = com.CreateParameter();
                    parameter.ParameterName = par.Key;
                    parameter.Value = par.Value;
                    parameter.Direction = System.Data.ParameterDirection.Input;

                    com.Parameters.Add(parameter);
                }

                return await com.ExecuteReaderAsync();
            }
            catch (Exception pEx)
            {
                String mensagem = String.Format("Mensagem:{0}\nSQL:{1}\nParametros:{2}", pEx.Message, pSql.ToString(), parametros.ToString());
                throw new Exception(mensagem, pEx);
            }

        }

        public async static Task<DbDataReader> ExecuteReaderFirst(DbCommand com, StringBuilder pSql, Dictionary<string, object> parametros, DbConnection FBcon, DbTransaction trans,
            Int32 First, Int32? Skip)
        {
            if (First > 0)
            {
                if (com.GetType().Name == "FbCommand")
                {
                    String _skip = String.Empty;
                    if (Skip.HasValue && Skip.Value > 1)
                        _skip = $"SKIP {Skip.Value}";

                    pSql = new StringBuilder(StringService.ReplaceFirst(pSql.ToString(), "SELECT ", $"SELECT FIRST {First} {_skip} "));
                }
                else if (com.GetType().Name == "MySqlCommand")
                {
                    String _skip = String.Empty;
                    if (Skip.HasValue && Skip.Value > 1)
                        _skip = $"OFFSET {Skip.Value}";

                    pSql = pSql.AppendLine($"LIMIT { First} {_skip}");
                }
            }
            return await ExecuteReader(com, pSql, parametros, FBcon, trans);
        }

        public async static Task<DbDataReader> ExecuteReaderPage(DbCommand com, StringBuilder pSql, Dictionary<string, object> parametros, DbConnection FBcon, DbTransaction trans,
            Int32 Page)
        {
            Int32 First = CONSTANTES_CORE.PaginacaoRegistros;
            Int32? Skip = Page == 1 ? 0 : CONSTANTES_CORE.PaginacaoRegistros * (Page - 1);

            return await ExecuteReaderFirst(com, pSql, parametros, FBcon, trans, First, Skip);
        }

        public async static Task<DbDataReader> ExecuteReaderMultiBanco(DbCommand com, StringBuilder pSqlFirebird, StringBuilder pSqlMySql, Dictionary<string, object> parametros, DbConnection FBcon, DbTransaction trans)
        {

            if (com.GetType().Name == "FbCommand")
            {
                return await ExecuteReader(com, pSqlFirebird, parametros, FBcon, trans);
            }
            else if (com.GetType().Name == "MySqlCommand")
            {
                return await ExecuteReader(com, pSqlMySql, parametros, FBcon, trans);
            }
            else
            {
                return await ExecuteReader(com, pSqlFirebird, parametros, FBcon, trans);
            }

        }

        public async static Task<DbDataReader> ExecuteReader(DbCommand com, StringBuilder pSql, Dictionary<string, object> parametros, DbConnection FBcon, DbTransaction trans)
        {
            if (FBcon == null)
                throw new Exception(String.Format("Coneção não está criada {0}", pSql));

            if (com == null)
                throw new Exception(String.Format("Driver de conexão não criou connextion {0}", pSql));

            if (FBcon.State == System.Data.ConnectionState.Closed) FBcon.Open();

            try
            {
                com.CommandText = pSql.ToString();
                com.Transaction = trans;

                com.Parameters.Clear();
                foreach (KeyValuePair<string, object> par in parametros)
                {
                    var parameter = com.CreateParameter();
                    parameter.ParameterName = par.Key;
                    parameter.Value = par.Value;
                    parameter.Direction = System.Data.ParameterDirection.Input;

                    com.Parameters.Add(parameter);
                }

                return await com.ExecuteReaderAsync();
            }
            catch (Exception pEx)
            {
                String mensagem = String.Format("Mensagem:{0}\nSQL:{1}\nParametros:{2}", pEx.Message, pSql.ToString(), parametros.ToString());
                throw new Exception(mensagem, pEx);
            }
        }



        #endregion

        #region ExecuteNumQuery

        public async static Task<Int32> ExecuteNonQuery(DbCommand com, StringBuilder pSql, DbConnection FBcon, DbTransaction trans)
        {
            return await ExecuteNonQuery(com, pSql, new Dictionary<string, object>(), FBcon, trans);
        }

        public async static Task<Int32> ExecuteNonQuery(DbCommand com, StringBuilder pSql, Dictionary<string, object> parametros, DbConnection FBcon, DbTransaction trans)
        {
            if (FBcon == null)
                throw new Exception(String.Format("Coneção não está criada {0}", pSql));

            if (com == null)
                throw new Exception(String.Format("Driver de conexão não criou connextion {0}", pSql));

            if (FBcon.State == System.Data.ConnectionState.Closed) FBcon.Open();

            com.CommandText = pSql.ToString();
            com.Transaction = trans;
            com.Parameters.Clear();

            foreach (KeyValuePair<string, object> par in parametros)
            {
                var parameter = com.CreateParameter();
                parameter.ParameterName = par.Key;
                parameter.Value = par.Value;
                parameter.Direction = System.Data.ParameterDirection.Input;

                com.Parameters.Add(parameter);
            }

            return await com.ExecuteNonQueryAsync();

        }

        public async static Task<Int32> ExecuteNonQuery(DbCommand com, StringBuilder pSql, Dictionary<string, string> parametros, DbConnection FBcon, DbTransaction trans)
        {
            if (FBcon == null)
                throw new Exception(String.Format("Coneção não está criada {0}", pSql));

            if (com == null)
                throw new Exception(String.Format("Driver de conexão não criou connextion {0}", pSql));

            if (FBcon.State == System.Data.ConnectionState.Closed) FBcon.Open();

            com.CommandText = pSql.ToString();
            com.Transaction = trans;
            foreach (KeyValuePair<string, string> par in parametros)
            {
                var parameter = com.CreateParameter();
                parameter.ParameterName = par.Key;
                parameter.Value = par.Value;
                parameter.Direction = System.Data.ParameterDirection.Input;

                com.Parameters.Add(parameter);
            }

            return await com.ExecuteNonQueryAsync();
        }

        public async static Task<Int64> ExecuteNonQueryReturning(DbCommand com, StringBuilder pSql, Dictionary<string, object> parametros, DbConnection FBcon, DbTransaction trans)
        {
            if (FBcon == null)
                throw new Exception(String.Format("Coneção não está criada {0}", pSql));

            if (com == null)
                throw new Exception(String.Format("Driver de conexão não criou connextion {0}", pSql));

            if (FBcon.State == System.Data.ConnectionState.Closed) FBcon.Open();

            com.CommandText = pSql.ToString();
            com.Transaction = trans;
            foreach (KeyValuePair<string, object> par in parametros)
            {
                var parameter = com.CreateParameter();
                parameter.ParameterName = par.Key;
                parameter.Value = par.Value;
                parameter.Direction = System.Data.ParameterDirection.Input;

                com.Parameters.Add(parameter);
            }

            var paramOut = com.CreateParameter();
            paramOut.ParameterName = "Id";
            paramOut.Direction = System.Data.ParameterDirection.Output;

            com.Parameters.Add(paramOut);
            await com.ExecuteNonQueryAsync();

            long retorno = paramOut.Value as long? ?? default(long);

            return retorno;
        }

        #endregion

        public static T convertReturnValue<T>(object value, object padrao)
        {
            var valor = value;
            if ((string.IsNullOrEmpty(valor?.ToString())) || (valor == null)) return (T)padrao;
            else if (typeof(T) == typeof(Double)) valor = Convert.ToDouble(valor);
            else if (typeof(T) == typeof(Decimal)) valor = Convert.ToDecimal(valor);
            else if (typeof(T) == typeof(Int16)) valor = Convert.ToInt16(valor);
            else if (typeof(T) == typeof(Int32)) valor = Convert.ToInt32(valor);
            else if (typeof(T) == typeof(Int64)) valor = Convert.ToInt64(valor);
            else if ((typeof(T) == typeof(Boolean)) && (valor.GetType() == typeof(String)))
            {
                valor = new String[] { "S", "SIM", "T", "TRUE" }.Contains(valor.ToString().ToUpper());
            }

            return (T)valor;
        }
    }
}
