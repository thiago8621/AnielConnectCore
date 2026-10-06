using System;

namespace Sinapse.Core.AspNetCore
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
