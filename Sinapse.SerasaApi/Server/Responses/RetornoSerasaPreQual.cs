using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sinapse.SerasaApi.Server.Responses
{
    public class Applicant
    {
        public string CNPJ { get; set; }
    }

    public class CDL
    {
        public string AllowList { get; set; }
        public string BlockList { get; set; }
    }

    public class Data
    {
        [JsonProperty("DV-Application")]
        public DVApplication DVApplication { get; set; }

        [JsonProperty("DV-Applicant")]
        public DVApplicant DVApplicant { get; set; }
        public WorkingStorage WorkingStorage { get; set; }

        [JsonProperty("DV-Events")]
        public DVEvents DVEvents { get; set; }

        [JsonProperty("DV-Results")]
        public DVResults DVResults { get; set; }
    }

    public class DecisaoProposta
    {
        public string DecisaoElegibilidadeCredito { get; set; }
        public string DataSistemicaDecisao { get; set; }
        public string HoraSistemicaDecisao { get; set; }
    }

    public class DVApplicant
    {
        public List<Applicant> Applicant { get; set; }
    }

    public class DVApplication
    {
        public string NumeroProposta { get; set; }
        public string DataProposta { get; set; }
        public string HoraProposta { get; set; }
        public string IDservico { get; set; }
        public string Fonte { get; set; }
        public string Canal { get; set; }
        public string Loja { get; set; }
        public string NomeRecomendacaoProposta { get; set; }
        public string CodigoRecomendacaoProposta { get; set; }
        public DecisaoProposta DecisaoProposta { get; set; }
        public CDL CDL { get; set; }
        public string VersaoSolucao { get; set; }
    }

    public class DVEvents
    {
        public List<Event> Events { get; set; }
    }

    public class DVResults
    {
        public List<MergedResult> MergedResults { get; set; }
    }

    public class Event
    {
        public string CodigoEvento { get; set; }
        public string MensagemEvento { get; set; }
    }

    public class MergedResult
    {
        public ResultadoDecisao ResultadoDecisao { get; set; }
    }

    public class ResultadoDecisao
    {
        public string MensagemDecisao { get; set; }
        public string ClassificacaoDecisao { get; set; }
    }

    public class ResponsePreQual
    {
        public string serviceContextId { get; set; }
        public Data data { get; set; }
    }

    public class WorkingStorage
    {
        public string CodigoErro { get; set; }
        public string MensagemErro { get; set; }
        public string FonteErro { get; set; }
        public string ContagemErros { get; set; }
    }

    public class RequestAuth
    {
        public string username { get; set; }
        public string password { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }

    public class ResponseAuth
    {
        public string issued_at { get; set; }
        public string expires_in { get; set; }
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
    }
}