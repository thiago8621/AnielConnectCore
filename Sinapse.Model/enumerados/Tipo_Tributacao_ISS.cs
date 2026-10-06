using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Tipo_Tributacao_ISS
    {
        [Display(Name = "ISENTO_ISS", ResourceType = typeof(Resources.RsFinanceiro))]
        Isento_ISS = 1,
        [Display(Name = "IMUNE", ResourceType = typeof(Resources.RsFinanceiro))]
        Imune = 2,
        [Display(Name = "NAO_INCIDENCIA_MUNICIPIO", ResourceType = typeof(Resources.RsFinanceiro))]
        Nao_Incidencia_Municipio = 3,
        [Display(Name = "NAO_TRIBUTAVEL", ResourceType = typeof(Resources.RsFinanceiro))]
        Nao_Tributavel = 4,
        [Display(Name = "RETIDO", ResourceType = typeof(Resources.RsFinanceiro))]
        Retido = 5,
        [Display(Name = "TRIBUTAVEL_DENTRO_MUNICIPIO", ResourceType = typeof(Resources.RsFinanceiro))]
        Tributavel_Dentro_Do_Municipio = 6,
        [Display(Name = "TRIBUTAVEL_FORA_MUNICIPIO", ResourceType = typeof(Resources.RsFinanceiro))]
        Tributavel_Fora_Do_Municipio = 7
    }
}
