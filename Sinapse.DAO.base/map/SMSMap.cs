using Sinapse.Model;
using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.map
{
    public class SMSMap : ClassMapBase<SMS>
    {
        public SMSMap() : base()
        {
            Table("TB_SMS");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_SMS");

            Map(u => u.Enviado, "ENVIADO");
            Map(u => u.Mensagem, "MENSAGEM").Length(5000);
            Map(u => u.Num_Celular, "NUMERO_CELULAR").Length(20);
            Map(u => u.Tipo_Integracao).CustomType<Tipo_Integracao_SMS>();
        }
    }
}
