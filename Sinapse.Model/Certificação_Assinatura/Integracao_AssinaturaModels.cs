

using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Model
{
    public class Enviar_Documento_ResponseModel
    {
        public virtual Int64? Id_Venda { get; set; }
        public virtual Int64? Id_Cliente { get; set; }
        public virtual string Id_Documento_Integracao { get; set; }
        public virtual string Token_Signatario { get; set; }
        public virtual Int64? Id_Signatario { get; set; }
        public virtual string Link_Assinatura { get; set; }
        public virtual TipoIntegracaoAssinaturaEnum Tipo_Integracao { get; set; }
        public virtual DateTime Data_Envio_Documento { get; set; }
    }
    public class Consultar_Documento_ResponseModel
    {
        public virtual Int64? Id_Venda { get; set; }
        public virtual Int64? Id_Cliente { get; set; }
        public virtual Int64? Id_Contrato { get; set; }
        public virtual string Id_Documento_Integracao { get; set; }
        public virtual bool IsAssinado { get; set; }
        public virtual string Link_Download_Documento { get; set; }
        public virtual string Token_Signatario { get; set; }
        public virtual Int64? Id_Signatario { get; set; }
        public virtual TipoIntegracaoAssinaturaEnum Tipo_Integracao { get; set; }
        public virtual DateTime Data_Assinatura { get; set; }

    }
}