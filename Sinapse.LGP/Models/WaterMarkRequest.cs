using System;

namespace Sinapse.LGPD.Models
{
    public class WatermarkRequest
    {
        public string NomeEmpresa { get; set; }
       
        public DateTime DataVisualizacao { get; set; }

        // Propriedade para facilitar a montagem do texto final da marca-d'água
        public string FullWatermarkText
        {
            get
            {
                return $"{NomeEmpresa} - {DataVisualizacao:dd/MM/yyyy HH:mm:ss}";
            }
        }
    }
}