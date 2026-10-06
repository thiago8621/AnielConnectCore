using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.IntegracoesERP.Ixc.Models
{
    public class RequestIXC
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string oper { get; set; }
        public string page { get; set; }
        public string rp { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
        public string grid_param { get; set; }
    }

    public class ResponseIXC<T> where T : class
    {
        public string page { get; set; }
        public string total { get; set; }
        public List<T> registros { get; set; }
    }

    public class ResponsePostIXC
    {
        public string type { get; set; }
        public string message { get; set; }
        public string id { get; set; }
    }
}