using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sinapse.Integracao.Roteamento.Models
{
    public class OSRMTableResponseModel
    {

        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("distances")]
        public IList<IList<double>> distances { get; set; }

        [JsonProperty("destinations")]
        public IList<Destination> destinations { get; set; }

        [JsonProperty("sources")]
        public IList<Source> sources { get; set; }
    }

    public class Destination
    {

        [JsonProperty("hint")]
        public string hint { get; set; }

        [JsonProperty("distance")]
        public double distance { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("location")]
        public IList<double> location { get; set; }
    }

    public class Source
    {

        [JsonProperty("hint")]
        public string hint { get; set; }

        [JsonProperty("distance")]
        public double distance { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("location")]
        public IList<double> location { get; set; }
    }

}