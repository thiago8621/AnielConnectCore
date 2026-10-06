using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public class DiferentObjectsException : Exception
    {
        public DiferentObjectsException ()
        {

        }

        public DiferentObjectsException (string message)
            : base(message)
        {

        }
        public DiferentObjectsException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
