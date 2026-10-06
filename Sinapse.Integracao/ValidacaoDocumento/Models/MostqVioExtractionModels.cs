using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.MostqVioExtractionModels.Models
{
    public class Crop
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public double score { get; set; }
        public string value { get; set; }
    }

    public class Result
    {
        public List<Crop> crops { get; set; }
        public List<Field> fields { get; set; }
        public int pageNumber { get; set; }
        public List<string> tags { get; set; }
    }

    public class Status
    {
        public string message { get; set; }
        public string code { get; set; }
        public object errors { get; set; }
    }

    public class VioExtractionResultResponse
    {
        public List<Result> result { get; set; }
        public string requestId { get; set; }
        public int elapsedMilliseconds { get; set; }
        public Status status { get; set; }
    }


    public class VioExtractionRequest
    {
        //Em caso de mais de um parâmetro, segue prioridade: qrCodeField, fileBase64 e fileUrl
        public string qrCodeField { get; set; }
        public string fileBase64 { get; set; }
        public object fileUrl { get; set; }
    }

}