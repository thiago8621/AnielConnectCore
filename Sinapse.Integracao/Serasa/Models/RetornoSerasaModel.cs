using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.Serasa
{
    public class RetornoCpfSerasaModel
    {
        public string LayoutString { get; set; }
        public string Cpf { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public bool Sucesso { get; set; }
        public string ErroMensagem { get; set; }
        public string RetornoString { get; set; }
       
    }
     public class RetornoCnpjSerasaModel
    {
        public string LayoutString { get; set; }
        public string Cnpj { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public bool Sucesso { get; set; }
        public string ErroMensagem { get; set; }
        public string RetornoString { get; set; }
       
    }

    public class ResponseConsulta
    {
        public string RetornoConsultaString { get; set; }
    }

  
}