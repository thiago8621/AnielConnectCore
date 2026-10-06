using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Status_ProspectoAniel
    {
        [Display(Name = "LEAD_NAO_QUALIFICADO", ResourceType = typeof(Resources.RsCadastros))]
        Lead_Nao_Qualificado = 0,
        [Display(Name = "LEAD_QUALIFICADO", ResourceType = typeof(Resources.RsCadastros))]
        Lead_Qualificado = 1,
        [Display(Name = "INVIABILIDADE_TECNICA", ResourceType = typeof(Resources.RsCadastros))]
        Inviabilidade_Tecnica = 2,
        [Display(Name = "RESTRICAO_COMERCIAL", ResourceType = typeof(Resources.RsCadastros))]
        Em_Negociacao = 3,
        [Display(Name = "EFETIVADO", ResourceType = typeof(Resources.RsCadastros))]
        Efetivado = 4,
        [Display(Name = "INATIVO", ResourceType = typeof(Resources.RsCadastros))]
        Inativo = 5,
        [Display(Name = "LEAD_COM_RESTRICAO", ResourceType = typeof(Resources.RsCadastros))]
        Lead_Com_Restricao = 6
    }
}
