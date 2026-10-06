using Aniel.Connect.Core;
using Aniel.Connect.Core.Service;
using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Aniel.Connect.Models
{

    public class MessengerEnvioModels
    {

        public class Recipient
        {
            public string id { get; set; }
        }
        public class Payload
        {
            public bool is_reusable { get; set; }
            public string url { get; set; }
            public string attachment_id { get; set; }
        }
        public class Attachment
        {
            public string type { get; set; }
            public Payload payload { get; set; }
        }
        public class Message
        {
            public string text { get; set; }
            public Attachment attachment { get; set; }
        }

        public class Root
        {
            public string messaging_type { get; set; }
            public Recipient recipient { get; set; }
            public Message message { get; set; }
        }


        public class RootMidia
        {
            public Recipient recipient { get; set; }
            public Message message { get; set; }
        }

    }
}