using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
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
