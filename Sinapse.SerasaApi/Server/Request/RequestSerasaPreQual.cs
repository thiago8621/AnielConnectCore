using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sinapse.SerasaApi.Server.Request
{
    public class Applicant
    {
        public string CPF { get; set; }
        public string CNPJ { get; set; }
    }

    public class CDL
    {
        public string BlockList { get; set; }
        public string AllowList { get; set; }
    }

    public class DVApplicant
    {
        public List<Applicant> Applicant { get; set; }
    }

    public class DVApplication
    {
        public string Fonte { get; set; }
        public string Canal { get; set; }
        public string IDservico { get; set; }
        public CDL CDL { get; set; }
        public string Loja { get; set; }
    }

    public class RequestPreQual
    {
        [JsonProperty("DV-Application")]
        public DVApplication DVApplication { get; set; }

        [JsonProperty("DV-Applicant")]
        public DVApplicant DVApplicant { get; set; }
    }


}