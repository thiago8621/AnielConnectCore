using System;

namespace Sinapse.DAO.AspNetCore
{
    public class PersistenceException : Exception
    {
        private Object modelo;

        public PersistenceException(string message, Exception inner, Object modelo)
            : base(message, inner)
        {
            this.modelo = modelo;
        }
    }
}
