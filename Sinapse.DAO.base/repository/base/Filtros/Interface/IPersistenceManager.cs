using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using Sinapse.Model;
using Sinapse.DAO.repository;
using Sinapse.Model.nhibernate;
using Sinapse.Model.core;

namespace Sinapse.DAO
{
    public interface IPersistenceManager
    {

        /// <summary>
        /// Deleta o objeto passado como parametro no banco de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do dominio a ser deletado.</typeparam>
        /// <param name="obj">Objeto de dominio a ser deletado.</param>
        /// <returns>Retorna true se o objeto for deletado com sucesso.</returns>        
        bool Delete<T>(T obj) where T : IModel;

        /// <summary>
        /// Deleta o objeto passado como parametro no banco de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do dominio a ser deletado.</typeparam>
        /// <param name="obj">Objeto de dominio a ser deletado.</param>
        /// <returns>Retorna true se o objeto for deletado com sucesso.</returns>        
        bool DeleteObject(object obj);

        ISession GetSession();


        IStatelessSession GetStatelessSession();

        /// <summary>
        /// Retorna um objeto do banco de dados de acordo com seu ID.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser retornado.</typeparam>
        /// <param name="oid">ID do objeto a ser retornado.</param>
        /// <returns>Objeto do tipo especificado no generics.</returns>
        T GetById<T>(Int64 oid) where T : IModel;

        bool InsertOrUpdate<T>(T obj) where T : IModel;

        bool InsertBatch<T>(T obj) where T : IModel;

        bool UpdateBatch<T>(T obj) where T : IModel;

        bool Save(Type tipo, object obj);

        bool Update(Type tipo, object obj);


        object GetById(Type tipo, Int64 oid);

        object GetByIdStateless(Type tipo, Int64 oid);

        /// <summary>
        /// Retorna uma lista de objetos do banco de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do da lista a ser retornada.</typeparam>
        /// <returns>Lista dos objetos do tipo passado como parametro.</returns>
        IList<T> GetAll<T>() where T : IModel;


        /// <summary>
        /// Retorna um objeto baseado em um exemplo passado como parametro.
        /// Sera retornado o primeiro objeto que atender aos criterios de busca estabelecidos 
        /// no exemplo, ou seja, se for passado um objeto com a propriedade "nome" preenchida,
        /// o metodo ira buscar na base de dados um objeto do tipo especificado que tenha o nome
        /// igual ao nome especificado no exemplo.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser buscado.</typeparam>
        /// <param name="objEx">Objeto de exemplo a ser localizado.</param>
        /// <returns>Retorna o objeto completo, caso nao encontre retorna null</returns>
        T GetObjectByExample<T>(T objEx) where T : IModel;

        /// <summary>
        /// Retorna uma lista de objetos baseado em um exemplo passado como parametro.
        /// Sera retornada uma lista de objetos, do tipo especificado no generic,
        /// que atender aos criterios de busca estabelecidos no exemplo, ou seja, 
        /// se for passado um objeto com a propriedade "nome" preenchida,
        /// o metodo ira buscar na base de dados todos os objetos do tipo especificado que tenham o nome
        /// igual ao nome descrito no exemplo.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser buscado.</typeparam>
        /// <param name="objEx">Objeto de exemplo a ser localizado.</param>
        /// <returns>Retorna a lista de objetos, caso nao encontre retorna uma lista vazia</returns>
        IList<T> GetListByExample<T>(T objEx) where T : IModel;

        /// <summary>
        /// Retorna uma lista de objetos baseado em um exemplo passado como parametro.
        /// Sera retornada uma lista de objetos, do tipo especificado no generic,
        /// que atender aos criterios de busca estabelecidos no exemplo, ou seja, 
        /// se for passado um objeto com a propriedade "nome" preenchida,
        /// o metodo ira buscar na base de dados todos os objetos do tipo especificado que tenham o nome
        /// igual ao nome descrito no exemplo.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser buscado.</typeparam>
        /// <param name="objEx">Objeto de exemplo a ser localizado.</param>
        /// <param name="maxResults">Numero maximo de registros a ser retornado.</param>
        /// <returns>Retorna a lista de objetos, caso nao encontre retorna uma lista vazia</returns>        
        IList<T> GetListByExample<T>(T objEx, int maxResults) where T : IModel;

        /// <summary>
        /// Retorna uma lista ordenada de objetos baseado em um exemplo passado como parametro.
        /// Sera retornada uma lista de objetos, do tipo especificado no generic,
        /// que atender aos criterios de busca estabelecidos no exemplo, ou seja, 
        /// se for passado um objeto com a propriedade "nome" preenchida,
        /// o metodo ira buscar na base de dados todos os objetos do tipo especificado que tenham o nome
        /// igual ao nome descrito no exemplo.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser buscado.</typeparam>
        /// <param name="objEx">Objeto de exemplo a ser localizado.</param>
        /// <returns>Retorna a lista de objetos, caso nao encontre retorna uma lista vazia</returns>
        IList<T> GetOrdedListByExample<T>(T objEx, OrdenacaoLista ordem, params string[] fieldNames) where T : IModel;

        /// <summary>
        /// Retorna um objeto a partir do filtro de um campo especificado no parametro.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser localizado.</typeparam>
        /// <param name="fieldName">Nome do campo de busca.</param>
        /// <param name="value">Valor do campo de busca.</param>
        /// <returns>Retorna o objeto completo de acordo com o filtro especificado, 
        /// caso nao encontre sera retornado null</returns>
        T GetObjectByField<T>(String fieldName, Object value) where T : IModel;

        /// <summary>
        /// Retorna uma lista de objetos a partir do filtro de um campo especificado no parametro.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser localizado.</typeparam>
        /// <param name="fieldName">Nome do campo de busca.</param>
        /// <param name="value">Valor do campo de busca.</param>
        /// <returns>Retorna uma lista de objetos de acordo com o filtro especificado, 
        /// caso nao encontre sera retornado uma lista vazia</returns>
        IList<T> GetListByField<T>(String fieldName, Object value) where T : IModel;

        IList<T> GetListByField<T>(String fieldName, Object value, SearchType searchType) where T : IModel;

        IList<T> GetListByField<T>(String fieldName, Object value, SearchType searchType, int MaxResult) where T : IModel;

        IList<object> GetListByField(Type tipo, String fieldName, Object value, SearchType searchType);

        IList<object> GetList(Type tipo, int MaxResults);

        bool InsertOrUpdate(Type tipo, Object obj);

        bool InsertOrUpdateInTransaction(Type tipo, object obj);

        IList<T> GetListByFiltro<T>(Filters filtros) where T : IModel;

        IList<Object> GetListByFiltro(Type tipo, Filters filtros);

        IList<T> GetListByFiltroPage<T>(Filters filtros, Int32 pageIndex, Int32 pageSize) where T : IModel;

        IList<Object> GetListByFiltroPage(Type tipo, Filters filtros, Int32 pageIndex, Int32 pageSize);

        T GetObjectByFiltro<T>(Filters filtros) where T : IModel;

        T GetObjectByFiltro<T>(Filters filtros, FetchMode mode, String EaggerProperty) where T : IModel;

        Object GetObjectByField(Type tipo, String fieldName, Object value);

        ITransaction BeginTransaction();

        ITransaction GetTransaction();

        bool DeleteInTransaction(object obj);

        T GetObjectByUnikeyField<T>(String fieldName, Object value) where T : IModel;

        void SetFlushMode(FlushMode pFlushMode);

        Boolean ExecuteDDL(String pScript);

        byte[] ConvertFileToArrayByte(String pCaminhoFile);

        void FilterToCriteria<T>(ICriteria criterio, Filters filtros) where T : IModel;

        void FilterToCriteria(Type tipo, ICriteria criterio, Filters filtros);
    }
}
