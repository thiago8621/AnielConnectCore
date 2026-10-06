using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.ReconhecimentoFacial.Models
{
    public class Result
    {
        public List<double> distances { get; set; }
        public double similarity { get; set; }
    }

    public class FaceCompareResultResponse
    {
        public Result result { get; set; }
        public string requestId { get; set; }
        public int elapsedMilliseconds { get; set; }
        public Status status { get; set; }
    }

    public class Status
    {
        public string message { get; set; }
        public string code { get; set; }
        public object errors { get; set; }
    }

    public class FaceCompareRequest
    {
        public string faceFileA { get; set; }
        public string faceFileB { get; set; }
        public string faceFileBase64A { get; set; }
        public string faceFileBase64B { get; set; }

    }
}