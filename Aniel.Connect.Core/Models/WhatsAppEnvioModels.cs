using Aniel.Connect.Core;
using Aniel.Connect.Core.Service;
using Newtonsoft.Json;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Aniel.Connect.Models
{

    public class WhatsAppEnvioModels
    {
        public class WhatsAppMessage
        {
            public string MessagingProduct { get; set; }
            public string To { get; set; }
            public string Type { get; set; }
            public Text Text { get; set; }
        }

        public class Text
        {
            [JsonProperty("body")]
            public string Body { get; set; }
        }

    }
}