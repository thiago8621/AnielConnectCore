using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.repository
{
    public class Procedure<T> : IDisposable where T : class
    {
        private IQuery query;

        private ISession _session;

        public Procedure(ISession session)
        {
            _session = session;
        }

        public Procedure(string procName)
        {
            query = _session.GetNamedQuery(procName);
        }

        public void setDouble(string nome, double valor)
        {
            query.SetDouble(nome, valor);
        }

        public void setString(string nome, string valor)
        {
            query.SetString(nome, valor);
        }

        public void setInt(string nome, Int64 valor)
        {
            query.SetInt64(nome, valor);
        }

        public void setBinary(string nome, byte[] valor)
        {
            query.SetBinary(nome, valor);
        }

        public List<T> Select()
        {
            try
            {
                query.SetResultTransformer(new NHibernate.Transform.AliasToBeanConstructorResultTransformer(typeof(T).GetConstructors()[0]));
                return new List<T>(query.List<T>());
            }
            catch
            {
                this.Execute();
                return null;
            }
        }

        public void Execute()
        {
            query.ExecuteUpdate();
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
