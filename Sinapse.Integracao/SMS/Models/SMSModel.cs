using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.SMS.Models
{
    public class EnviarSMS_RequestModel
    {
        public virtual bool Salvar_Em_Banco { get; set; }
        public virtual string Mensagem { get; set; }
        public virtual string Num_Celular { get; set; }
        public virtual bool isReenvio { get; set; }
        public virtual Int64? Id_Integracao { get; set; }
    }

    public class EnviarSMS_ResponseModel
    {
        public virtual Int64? Id_Integracao { get; set; }
        public virtual bool Sucesso { get; set; }
        public virtual string MensagemErro { get; set; }
        public virtual List<String> MensagensErro { get; set; }
        public virtual string Mensagem { get; set; }
    }

    public class EnviarLoteSMS_RequestModel
    {
        public virtual bool Salvar_Em_Banco { get; set; }
        public virtual List<EnviarSMS_RequestModel> Mensagens { get; set; }
    }

    public class EnviarLoteSMS_ResponseModel
    {
        public virtual bool Sucesso { get; set; }
        public virtual string MensagemErro { get; set; }
        public virtual List<EnviarSMS_ResponseModel> Retorno { get; set; }
    }
    public class BlipRequestModel 
    {
        public  Guid Id { get; set;}
        public string to { get; set;}
        public string type { get; set;}
        public string content { get; set;}
    }
}