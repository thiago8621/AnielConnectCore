using NHibernate;
using System;

namespace Sinapse.DAO.AspNetCore.repository
{
    public class Transaction<T> : IDisposable where T : class
    {
        private ISession _session;

        public Transaction(ISession session)
        {
            _session = session;
        }

        public void acao(Action metodo)
        {
            using (ITransaction transaction = _session.BeginTransaction())
            {
                try
                {                   
                    metodo();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    if (transaction.IsActive)
                        transaction.Rollback();
                    throw e;
                }
            }
        }

        public object Save(T obj)
        {
            return _session.Save(obj);
        }

        public void Update(T obj)
        {
            _session.Update(obj);
        }

        public void SaveOrUpdate(T obj)
        {
            _session.SaveOrUpdate(obj);
        }

        public void Remove(T obj)
        {
            _session.Delete(obj);
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
