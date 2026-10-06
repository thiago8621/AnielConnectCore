using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public class EAnielException : Exception
    {
        public EAnielException()
        {

        }
        
        public EAnielException(string message): base(message)
        {

        }

        public EAnielException(string message, System.Exception inner): base(message, inner)
        {

        }
    }
}
