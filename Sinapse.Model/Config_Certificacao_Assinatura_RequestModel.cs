
using Aniel.Connect.ModelLog.nhibernate;
using Resources;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using Sinapse.ModelLog;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    public class Config_Certificacao_Assinatura_RequestModel : BaseModel
    {

        public virtual String Token_Integracao_Assinatura { get; set; }

        public virtual TipoIntegracaoAssinaturaEnum? Tipo_Integracao_Assinatura { get; set; }

        public virtual bool Utiliza_Certificacao_Assinatura { get; set; }

        public virtual String Nome_Cliente_Sinapse { get; set; }

        public virtual String OLA_URL_Logo { get; set; }

        public virtual Boolean Exige_Foto_Termo { get; set; }

        public virtual Int64? Id_Cliente { get; set; }

        public virtual Int64? Id_Venda { get; set; }
        public virtual Int64? Id_Contrato { get; set; }
        
        public virtual string Token_Documento_Assinatura { get; set; }

        public virtual string Url_Assinar_Documento { get; set; }

        public virtual String NomeFantasia { get; set; }
        public virtual DateTime? Data_Nascimento { get; set; }

        public virtual String CpfCnpj { get; set; }

        public virtual String Email_Principal { get; set; }
        public virtual String Email_Auxiliar { get; set; }




    }

       
}
