using MongoDB.Driver;
using System;

namespace Sinapse.DAO.MongoDB
{
    public class ConexaoMongoGeography : IDisposable, IConexaoMongo
    {
        IMongoDatabase database = null;

        //Método para retornar uma conexão singleton com o MongoDb
        public IMongoDatabase GetConnection()
        {
            //singleton (instanciar 1 unica vez)
            try
            {
                String servidor = String.Empty;
                String porta = String.Empty;
                String banco = String.Empty;

                try
                {
                    servidor = CONSTANTES.BancoMongoGeograph.Split(':')[0];
                    porta = CONSTANTES.BancoMongoGeograph.Split(':')[1].Split('/')[0];
                    banco = CONSTANTES.BancoMongoGeograph.Split(':')[1].Split('/')[1];
                }
                catch
                {
                    throw new Exception("Caminho definido errado no config BancoMongo Ex.: LOCALHOST:27017/BANCO");
                }

                var settings = new MongoClientSettings
                {
                    ServerSelectionTimeout = new TimeSpan(0, 0, 10),
                    Server = new MongoServerAddress(servidor, Convert.ToInt32(porta))
                };

                MongoClient client = new MongoClient(settings);

                //Abrir conexão com uma base de dados
                database = client.GetDatabase(banco);
            }
            catch (Exception e)
            {
                database = null;
                if (e.InnerException != null && e.InnerException is TimeoutException)
                    Console.WriteLine($"Erro: Não foi possível conectar ao servidor (time out após 10 segundos).");
            }
            

            //retornar a conexão...
            return database;
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
