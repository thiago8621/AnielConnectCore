using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public static class UserConnectionFactory
    {
        public static IUserConnectionControl GetConnectionControl(TipoCC config)
        {
            switch (config)
            {
                case TipoCC.Win32: return new UserConnectionControlWin32();
                case TipoCC.Web: return new UserConnectionControlWeb();
                default: throw new InvalidUserConnectionControl();
            }
        }
    }
}
