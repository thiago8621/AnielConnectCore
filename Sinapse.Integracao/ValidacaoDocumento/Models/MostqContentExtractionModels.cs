using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.MostqContenctExtractionModels.Models
{
    public class Field
    {
        public string name { get; set; }
        public string value { get; set; }
        public double? score { get; set; }
    }

    public class ResultItem
    {
        public List<Field> fields { get; set; }
        public object image { get; set; } 
        public double? score { get; set; }
        public string type { get; set; }
        public int pageNumber { get; set; }
        public List<string> tags { get; set; }
        public List<object> tables { get; set; } 
    }

    public class Status
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class ContentExtractionResultResponse
    {
        public List<ResultItem> result { get; set; }
        public string requestId { get; set; }
        public int elapsedMilliseconds { get; set; }
        public Status status { get; set; }
    }

    public class ContentExtractionRequest
    {
        //Em caso de mais de um parâmetro, segue prioridade: qrCodeField, fileBase64 e fileUrl
        public string qrCodeField { get; set; }
        public string fileBase64 { get; set; }
        public object fileUrl { get; set; }
    }

}