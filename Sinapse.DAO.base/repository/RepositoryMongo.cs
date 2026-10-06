using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sinapse.DAO.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.Repository
{
    public class RepositoryMongo<T> : IDisposable where T : class
    {
        private String _collection;
        private IMongoDatabase db;

        public RepositoryMongo(IConexaoMongo conexaoMongo)
        {
            _collection = typeof(T).Name;
            db = conexaoMongo.GetConnection();
        }

        //Método para gravar 1 registro na base
        public async Task Save(T obj)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                await collection.InsertOneAsync(obj); //gravação...
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        //Método para gravar vários registros na base
        public async Task Save(List<T> obj)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                await collection.InsertManyAsync(obj); //gravação...
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        //listar todos os Produtos
        public async Task<List<T>> FindAll()
        {
            try
            {
                IMongoCollection<T> cursor = db.GetCollection<T>(_collection);

                //retornar os dados...
                return (await cursor.FindAsync<T>(FilterDefinition<T>.Empty)).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        public async Task<List<T>> FindAll(Expression<Func<T, bool>> filter)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                //retornar o resultado da consulta...
                return (await collection.FindAsync(filter)).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }
        public async Task<List<T>> FindAllWFilter(FilterDefinition<T> filter)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                //retornar o resultado da consulta...
                return (await collection.FindAsync(filter)).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        //Deleta um item específico dentro do array
        public async Task DeleteItemFromArray<TField>(Expression<Func<T, bool>> filter, Expression<Func<T, IEnumerable<TField>>> arrayField, Expression<Func<TField, bool>> arrayFilter)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                var update = Builders<T>.Update.PullFilter(arrayField, arrayFilter);

                await collection.UpdateOneAsync(filter, update);
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }


        public async Task<T> Update(Expression<Func<T, bool>> filter, T myObject)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);
                return await collection.FindOneAndReplaceAsync(filter, myObject);

            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        //buscar 1 Produto pelo filtro
        public async Task<T> Find(Expression<Func<T, bool>> filter)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                //retornar o resultado da consulta...
                return (await collection.FindAsync(filter)).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }
       
        public async Task<List<T>> FindPageAsync(FilterDefinition<T> filter, int skip, int limit)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                FindOptions<T> options = new FindOptions<T>();

                if (limit != -1)
                {
                    options.Skip = skip;
                    options.Limit = limit;
                }

                var retorno = (await collection.FindAsync(filter, options));
                //retornar o resultado da consulta...
                return retorno.ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }
        public async Task<long> GetCount(Expression<Func<T, bool>> filter)
        {
            try 
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                return (await collection.CountAsync<T>(filter));
            }
            catch (Exception ex)
            {
                throw new Exception("Erro: " + ex.Message);
            }
        }

        //Método para excluir 1 Produto
        public async Task Delete(Expression<Func<T, bool>> filter)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                //exclusão...
                await collection.DeleteOneAsync(filter);
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        public async Task DeleteAll()
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                //exclusão...
                await collection.DeleteManyAsync(FilterDefinition<T>.Empty);
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        public IMongoCollection<T> GetCollection()
        {
            try
            {
                return db.GetCollection<T>(_collection);
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        public IMongoQueryable<T> GetQueryable()
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);

                //exclusão...
                return collection.AsQueryable();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }

        /*public async Task<List<T>> FindPageAsync(FilterDefinition<T> filter, int skip, int limit)
        {
            try
            {
                IMongoCollection<T> collection = db.GetCollection<T>(_collection);
                var retorno = (await collection.FindAsync(filter, new FindOptions<T> { Skip = skip, Limit = limit }));
                //retornar o resultado da consulta...
                return retorno.ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e.Message);
            }
        }*/



        //Método para atualizar os dados do Produto
        //public async Task Edit(T obj)
        //{
        //    try
        //    {
        //        IMongoDatabase db = Conexao.GetConnection();
        //        IMongoCollection<T> collection = db.GetCollection<T>(_collection);

        //        //consulta para buscar 1 Produto no banco pelo id
        //        var consulta = Builders<T>.Filter.Eq("id", obj.id);

        //        //executando...
        //        await collection.ReplaceOneAsync(consulta, obj);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Erro: " + e.Message);
        //    }
        //}

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
