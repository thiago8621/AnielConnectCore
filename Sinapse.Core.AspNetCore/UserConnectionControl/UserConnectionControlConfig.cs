using System;

namespace Sinapse.Core.AspNetCore
{
    public static class UserConnectionControlConfig
    {
        public static TipoCC tipoConnection { get; set; }
        public static SistemaWeb sistema { get; set; }
        public static Boolean exibeErro { get; set; }
    }
}
