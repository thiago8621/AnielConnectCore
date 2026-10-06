
using NHibernate;
using Sinapse.Integracao.Certificacao_Assinatura.Interfaces;
using Sinapse.Model;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;

namespace Sinapse.Integracao.Certificacao_Assinatura.Repositorio
{
    public class IntegracaoAssinaturaRepository
    {
        public Consultar_Documento_ResponseModel Consultar_Documento(IntegracaoAssinatura integracao, ISession session, Config_Certificacao_Assinatura_RequestModel config)
        {
            return integracao.Consultar_Documento(session, config);
        }
        public Enviar_Documento_ResponseModel Enviar_Documento(IntegracaoAssinatura integracao, ISession session, Config_Certificacao_Assinatura_RequestModel config, String termo_adesao_base64)
        {
            return integracao.Enviar_Documento(session, config, termo_adesao_base64);
        }
        public String Get_Documento_Pdf(IntegracaoAssinatura integracao, Config_Certificacao_Assinatura_RequestModel config, String url_pdf)
        {
            return integracao.Get_Documento_Pdf(config, url_pdf);
        }
    }
}