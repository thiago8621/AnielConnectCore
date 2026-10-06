using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aniel.Connect.Integracoes.SerasaExperian.Models
{
    public class DescricoesErros
    {
        [JsonProperty("code")]
        public string Codigo { get; set; }        
        public string Descricao { get; set; }
        [JsonProperty("message")]
        public string Mensagem { get; set; }
    }
}