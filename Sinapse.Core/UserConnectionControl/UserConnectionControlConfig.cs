using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public static class UserConnectionControlConfig
    {
        public static TipoCC tipoConnection { get; set; }
        public static SistemaWeb sistema { get; set; }
        public static Boolean exibeErro { get; set; }
    }
}
