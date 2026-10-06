using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerado
{
    public enum Exigibilidade_ISS
    {
        [Display(Name = "EXIGIVEL", ResourceType = typeof(Resources.RsFinanceiro))]
        Exigivel = 1,
        [Display(Name = "NAO_INCIDENCIA", ResourceType = typeof(Resources.RsFinanceiro))]
        Nao_Incidencia = 2,
        [Display(Name = "ISENCAO", ResourceType = typeof(Resources.RsFinanceiro))]
        Isencao = 3,
        [Display(Name = "EXPORTACAO", ResourceType = typeof(Resources.RsFinanceiro))]
        Exportacao = 4,
        [Display(Name = "IMUNIDADE", ResourceType = typeof(Resources.RsFinanceiro))]
        Imunidade = 5,
        [Display(Name = "SUSPENSO_ACAO_JUDICIAL", ResourceType = typeof(Resources.RsFinanceiro))]
        Suspenso_Acao_Judicial = 6,
        [Display(Name = "SUSPENSO_ACAO_ADM", ResourceType = typeof(Resources.RsFinanceiro))]
        Suspenso_Acao_Administrativa = 7
    }
}
