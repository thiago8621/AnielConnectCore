using Npgsql;
using System;

namespace Sinapse.DAO.repository
{
    public class PostgreSqlUtil: IDisposable
{
    /// <summary>
    ///     Abre e retorna uma conexão com um banco PostgreSQL a partir da connection string informada
    /// </summary>
    /// <param name="connectionString">Connection string do banco PostgreSQL que será utilizado</param>
    /// <exception cref="ArgumentException"></exception>
    public static NpgsqlConnection GetConnection(string connectionString)
    {
        if (String.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("String de conexão não informada", "connectionString");
        }

        conn = new NpgsqlConnection(connectionString);
        if(conn.State != System.Data.ConnectionState.Open)
         conn.Open();

        return conn;
    }

        public void Dispose()
        {
            conn.Close();
        }

        public static NpgsqlConnection conn;
    }
}
