using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Sinapse.Model;
using Sinapse.DAO.Repository;
using Sinapse.DAO.repository;
using Sinapse.Model.nhibernate;
using Sinapse.Model.core;

namespace Sinapse.DAO
{
    public static class PersistenceFacade
    {
        private static IPersistenceManager persistenceManager = PersistenceAbstractFactory.GetPersistenceFactory().GetPersistenceManager();

        public static ITransaction GetTransaction()
        {
            return persistenceManager.GetTransaction();
        }

        public static ITransaction BeginTransaction()
        {
            return persistenceManager.BeginTransaction();
        }

        public static ISession GetSession()
        {
            return persistenceManager.GetSession();
        }

        public static IStatelessSession GetStatelessSession()
        {
            return persistenceManager.GetStatelessSession();
        }

        public static IList<T> GetListByExample<T>(T objEx) where T : IModel
        {
            return persistenceManager.GetListByExample<T>(objEx);
        }

        public static IList<T> GetListByExample<T>(T objEx, int maxResults) where T : IModel
        {
            return persistenceManager.GetListByExample<T>(objEx, maxResults);
        }

        public static T GetObjectByExample<T>(T objEx) where T : IModel
        {
            return persistenceManager.GetObjectByExample<T>(objEx);
        }

        public static T GetObjectByField<T>(String fieldName, Object value) where T : IModel
        {
            return persistenceManager.GetObjectByField<T>(fieldName, value);
        }

        public static IList<T> GetListByField<T>(String fieldName, Object value) where T : IModel
        {
            return persistenceManager.GetListByField<T>(fieldName, value);
        }

        public static IList<T> GetListByField<T>(String fieldName, Object value, SearchType searchType) where T : IModel
        {
            return persistenceManager.GetListByField<T>(fieldName, value, searchType);
        }

        public static IList<T> GetListByField<T>(String fieldName, Object value, SearchType searchType, int MaxResult) where T : IModel
        {
            return persistenceManager.GetListByField<T>(fieldName, value, searchType, MaxResult);
        }

        public static IList<Object> GetListByField(Type tipo, string fieldName, object value, SearchType searchType)
        {
            return persistenceManager.GetListByField(tipo, fieldName, value, searchType);
        }

        public static bool InsertOrUpdate<T>(T obj) where T : IModel
        {
            return persistenceManager.InsertOrUpdate<T>(obj);
        }

        public static bool InsertBatch<T>(T obj) where T : IModel
        {
            return persistenceManager.InsertBatch<T>(obj);
        }

        public static bool UpdateBatch<T>(T obj) where T : IModel
        {
            return persistenceManager.UpdateBatch<T>(obj);
        }

        public static bool InsertOrUpdate(Type tipo, Object obj)
        {
            return persistenceManager.InsertOrUpdate(tipo, obj);
        }

        public static bool Save(Type tipo, object obj)
        {
            return persistenceManager.Save(tipo, obj);
        }

        public static bool Update(Type tipo, object obj)
        {
            return persistenceManager.Update(tipo, obj);
        }

        public static Object GetObjectByField(Type tipo, String fieldName, Object value)
        {
            return persistenceManager.GetObjectByField(tipo, fieldName, value);
        }

        public static bool Delete<T>(T obj) where T : IModel
        {
            ModelIntegrityService.RunIntegrity(obj, OperationIntegrity.Delete);

            return persistenceManager.Delete<T>(obj);
        }

        public static bool DeleteObject(object obj)
        {
            ModelIntegrityService.RunIntegrity(obj, OperationIntegrity.Delete);

            return persistenceManager.DeleteObject(obj);
        }

        public static T GetById<T>(Int64 oid) where T : IModel
        {
            IPersistenceManager persistence = PersistenceAbstractFactory.GetPersistenceFactory().GetPersistenceManager();
            return persistence.GetById<T>(oid);
        }

        public static object GetById(Type tipo, Int64 oid)
        {
            IPersistenceManager persistence = PersistenceAbstractFactory.GetPersistenceFactory().GetPersistenceManager();
            return persistence.GetById(tipo, oid);
        }

        public static object GetByIdStateless(Type tipo, Int64 oid)
        {
            IPersistenceManager persistence = PersistenceAbstractFactory.GetPersistenceFactory().GetPersistenceManager();
            return persistence.GetByIdStateless(tipo, oid);
        }

        public static IList<T> GetAll<T>() where T : IModel
        {
            return persistenceManager.GetAll<T>();
        }

        public static IList<Object> GetList(Type tipo, int MaxResults)
        {
            return persistenceManager.GetList(tipo, MaxResults);
        }

        public static Boolean ExecuteDDL(String pScript)
        {
            return persistenceManager.ExecuteDDL(pScript);
        }

        public static byte[] ConvertFileToArrayByte(String fullFilePath)
        {
            return persistenceManager.ConvertFileToArrayByte(fullFilePath);
        }

        public static IList<T> GetOrdedListByExample<T>(T objEx, OrdenacaoLista ordem, params string[] fieldNames) where T : IModel
        {
            return persistenceManager.GetOrdedListByExample<T>(objEx, ordem, fieldNames);
        }

        public static IList<T> GetListByFiltro<T>(Filters filtros) where T : IModel
        {
            return persistenceManager.GetListByFiltro<T>(filtros);
        }

        public static IList<Object> GetListByFiltro(Type tipo, Filters filtros)
        {
            return persistenceManager.GetListByFiltro(tipo, filtros);
        }

        public static IList<T> GetListByFiltroPage<T>(Filters filtros, Int32 pageIndex, Int32 pageSize) where T : IModel
        {
            return persistenceManager.GetListByFiltroPage<T>(filtros, pageIndex, pageSize);
        }

        public static IList<Object> GetListByFiltroPage(Type tipo, Filters filtros, Int32 pageIndex, Int32 pageSize)
        {
            return persistenceManager.GetListByFiltroPage(tipo, filtros, pageIndex, pageSize);
        }

        public static T GetObjectByFiltro<T>(Filters filtros) where T : IModel
        {
            return persistenceManager.GetObjectByFiltro<T>(filtros);
        }

        public static T GetObjectByFiltro<T>(Filters filtros, FetchMode mode, String EaggerProperty) where T : IModel
        {
            return persistenceManager.GetObjectByFiltro<T>(filtros, mode, EaggerProperty);
        }

        public static Boolean InsertOrUpdateInTransaction(Type tipo, Object obj)
        {
            return persistenceManager.InsertOrUpdateInTransaction(tipo, obj);
        }

        public static Boolean DeleteInTransaction(object obj)
        {
            return DeleteInTransaction(obj);
        }

        public static void FilterToCriteria<T>(ICriteria criterio, Filters filtros) where T : IModel
        {
            persistenceManager.FilterToCriteria<T>(criterio, filtros);
        }

        public static void FilterToCriteria(Type tipo, ICriteria criterio, Filters filtros)
        {
            persistenceManager.FilterToCriteria(tipo, criterio, filtros);
        }

    }
}
