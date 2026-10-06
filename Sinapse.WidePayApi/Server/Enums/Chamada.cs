using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi.Server.Enums
{
    public class Chamada
    {
        static String url = "https://api.widepay.com";
        static String versao = "v1";

        private Chamada(string value) {
            Value = String.Format("{0}/{1}/{2}",url,versao, value);
        }

        public string Value { get; set; }

        public static Chamada Adicionar => new Chamada("recebimentos/cobrancas/adicionar");
        public static Chamada Notificacao => new Chamada("recebimentos/cobrancas/notificacao");
        public static Chamada Boleto => new Chamada("recebimentos/cobrancas/boleto");
        public static Chamada Consultar => new Chamada("recebimentos/cobrancas/consultar");
        public static Chamada Cancelar => new Chamada("recebimentos/cobrancas/cancelar");
    }
}
