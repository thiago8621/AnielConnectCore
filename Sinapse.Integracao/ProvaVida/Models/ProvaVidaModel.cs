using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sinapse.Integracao.ProvaVida.Models
{
    public class Result
    {
        public double globalScore { get; set; }
        public double movementScore { get; set; }
        public double livenessScore { get; set; }
        public double imageScore { get; set; }
        public string frontalImage { get; set; }
    }

    public class Status
    {
        public string code { get; set; }
        public string message { get; set; }
    }

    public class DetectResultResponse
    {
        public Result result { get; set; }
        public string requestId { get; set; }
        public string elapsedMilliseconds { get; set; }
        public Status status { get; set; }
    }

    public class DetectRequest
    {
        public string fileBase64 { get; set; }
        public string fileUrl { get; set; }
        public List<object> movements { get; set; } //Este campo deve ser vazio no caso de detecção de vida passiva. Será retornado erro 415 no caso de detecção com imagem e este campo não for vazio.
        public string returnFullFace { get; set; } //Imagem retornada será fullSize (true) ou reduzido (false). Esse parâmetro será false se não for inserido.
    }
}