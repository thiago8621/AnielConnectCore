
using NHibernate;
using Sinapse.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sinapse.Integracao.Certificacao_Assinatura.Interfaces
{
    public interface IntegracaoAssinatura
    {
        Consultar_Documento_ResponseModel Consultar_Documento(ISession session, Config_Certificacao_Assinatura_RequestModel config);
        Enviar_Documento_ResponseModel Enviar_Documento(ISession session, Config_Certificacao_Assinatura_RequestModel config, String termo_adesao_base64);
        Task Assinar_Documento(ISession session, Config_Certificacao_Assinatura_RequestModel config);
        String Get_Documento_Pdf(Config_Certificacao_Assinatura_RequestModel config, String url_pdf);        
    }
}