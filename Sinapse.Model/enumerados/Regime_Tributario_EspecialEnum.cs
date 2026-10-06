using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Regime_Tributario_EspecialEnum
    {
        [Display(Name = "SEM_REGIME_TRIBUTARIO_ESPECIAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Sem_Regime_Tributario_Especial = 0,
        [Display(Name = "MICRO_EMPRESA_MUNICIPAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Micro_Empresa_Municipal = 1,
        [Display(Name = "ESTIMATIVA", ResourceType = typeof(Resources.RsFinanceiro))]
        Estimativa = 2,
        [Display(Name = "SOCIEDADE_PROFISSIONAIS", ResourceType = typeof(Resources.RsFinanceiro))]
        Sociedade_Profissionais = 3,
        [Display(Name = "COOPERATIVA", ResourceType = typeof(Resources.RsFinanceiro))]
        Cooperativa = 4,
        [Display(Name = "MICROEMPRESARIO_INDIVIDUAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Micro_Empresario_Individual = 5,
        [Display(Name = "MICROEMPRESA_PEQUENO_PORTE", ResourceType = typeof(Resources.RsFinanceiro))]
        MicroEmpresa_Pequeno_Porte = 6
    }
}
