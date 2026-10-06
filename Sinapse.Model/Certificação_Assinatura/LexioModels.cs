using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Model
{
    public class Lexio_Enviar_Documento_ResponseModel
    {
        public Document_Enviar document { get; set; }
    }

    public class Document_Enviar
    {
        public string filename { get; set; }
        public string document_name { get; set; }
        public string document_token { get; set; }
        public Created_At created_at { get; set; }
        public bool finished { get; set; }
        public bool canceled { get; set; }
        public bool sequence { get; set; }
        public string service { get; set; }
        public string status { get; set; }
        public Extra_Values extra_values { get; set; }
        public string download { get; set; }
        public Signer[] signers { get; set; }
    }

    public class Lexio_Consultar_Documento_ResponseModel
    {
        public Document_Consultar document { get; set; }
    }

    public class Document_Consultar 
    {
        public string filename { get; set; }
        public string document_name { get; set; }
        public string document_token { get; set; }
        public Created_At created_at { get; set; }
        public bool finished { get; set; }
        public bool canceled { get; set; }
        public bool sequence { get; set; }
        public string service { get; set; }
        public string status { get; set; }
        public Extra_Values extra_values { get; set; }
        public string download { get; set; }
        public Signer[] signers { get; set; }
        public dynamic next_signer { get; set; }
        public object[] recorded_log { get; set; }
    }

    public class Created_At
    {
        public DateTime date { get; set; }
        public int timezone_type { get; set; }
        public string timezone { get; set; }
    }

    public class Signer
    {       
        public int id { get; set; }
        public string email { get; set; }
        public string nome { get; set; }
        public int order { get; set; }
        public string signer_token { get; set; }
        public string signer_link { get; set; }
        public Inserted inserted { get; set; }
        public Signed signed { get; set; }
        public Extra_Values extra_values { get; set; }
    }

    public class Inserted
    {
        public DateTime date { get; set; }
        public int timezone_type { get; set; }
        public string timezone { get; set; }
    }


    public class Extra_Values
    {
        public string ip { get; set; }
        public string rg { get; set; }
        public string cpf { get; set; }
        public object[] tag { get; set; }
        public string funcao { get; set; }
        public string hasDraw { get; set; }
        public string birthday { get; set; }
        public string completed_name { get; set; }
        public string resumo { get; set; }
        public int lembrete { get; set; }
        public object[] destinatarios { get; set; }
    }               

    public class Signed
    {
        public DateTime date { get; set; }
        public int timezone_type { get; set; }
        public string timezone { get; set; }
    }

}