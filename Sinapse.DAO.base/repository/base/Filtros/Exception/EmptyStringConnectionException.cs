using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO
{
    public class EmptyStringConnectionException : Exception
    {
        public EmptyStringConnectionException()
        {
        }

        public EmptyStringConnectionException(string message)
            : base(message)
        {
        }
        public EmptyStringConnectionException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
