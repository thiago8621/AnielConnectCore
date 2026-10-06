using NHibernate;
using Sinapse.Model.AspNetCore.core;
using System;
using System.Collections.Generic;

namespace Sinapse.DAO.AspNetCore.Repository
{
    public class GenericBuscaRepository : IDisposable
    {

        public static IStatelessSession GetStatelessSession()
        {
            return PersistenceFacade.GetStatelessSession();
        }

        public static ISession GetSession()
        {
            return PersistenceFacade.GetSession();
        }

        public static object GetObjectById(Type tipo, Int64 id)
        {
            return PersistenceFacade.GetById(tipo, id);
        }

        public static object GetByIdStateless(Type tipo, Int64 id)
        {
            return PersistenceFacade.GetByIdStateless(tipo, id);
        }

        public static IList<Object> GetListByFilters(Type tipo, string fieldName, object value, SearchType searchType)
        {
            Filters filtros = new Filters();
            filtros.AddFilter(fieldName, searchType, value);

            return PersistenceFacade.GetListByFiltro(tipo, filtros);
        }

        public static IList<Object> GetListByField(Type tipo, string fieldName, object value, SearchType searchType)
        {
            return PersistenceFacade.GetListByField(tipo, fieldName, value, searchType);
        }

        public static IList<Object> GetListByFilters(Type tipo, Filters filtros)
        {
            return PersistenceFacade.GetListByFiltro(tipo, filtros);
        }

        public static IList<Object> GetListByFiltersPage(Type tipo, Filters filtros, Int32 pageIndex, Int32 pageSize)
        {
            return PersistenceFacade.GetListByFiltroPage(tipo, filtros, pageIndex, pageSize);
        }

        public static object InsertOrUpdate(Type tipo, object obj)
        {
            return PersistenceFacade.InsertOrUpdate(tipo, obj);
        }

        public static bool Save(Type tipo, object obj)
        {
            return PersistenceFacade.Save(tipo, obj);
        }

        public static bool Update(Type tipo, object obj)
        {
            return PersistenceFacade.Update(tipo, obj);
        }

        public static object GetAll(Type tipo)
        {
            return PersistenceFacade.GetListByFiltro(tipo, new Filters());
        }

        public static object Delete(object objeto)
        {
            return PersistenceFacade.DeleteObject(objeto);
        }

        public static object GetObjectByField(Type tipo, String fieldName, Object value)
        {
            return PersistenceFacade.GetObjectByField(tipo, fieldName, value);
        }

        public static ITransaction GetTransaction()
        {
            return PersistenceFacade.GetTransaction();
        }

        public static Boolean ExecuteDDL(String pScript)
        {
            return PersistenceFacade.ExecuteDDL(pScript);
        }

        public static byte[] ConvertFileToArrayByte(String fullFilePath)
        {
            return PersistenceFacade.ConvertFileToArrayByte(fullFilePath);
        }

        public static Boolean InsertOrUpdateInTransaction(Type tipo, Object obj)
        {
            return PersistenceFacade.InsertOrUpdateInTransaction(tipo, obj);
        }

        public static Boolean DeleteInTransaction(object obj)
        {
            return PersistenceFacade.DeleteInTransaction(obj);
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
