using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Status_Contrato
    {
        [Display(Name = "SUSPENSO_DEBITO", ResourceType = typeof(Resources.RsCadastros))]
        Suspenso_Debito = 0,
        [Display(Name = "AGUARDANDO_ATIVACAO", ResourceType = typeof(Resources.RsCadastros))]
        Aguardando_Ativacao = 1,
        [Display(Name = "ATIVO", ResourceType = typeof(Resources.RsCadastros))]
        Ativo = 2,
        [Display(Name = "SUSPENSO_EMPRESA", ResourceType = typeof(Resources.RsCadastros))]
        Suspenso_Empresa = 3,
        [Display(Name = "CANCELADO", ResourceType = typeof(Resources.RsCadastros))]
        Cancelado = 4,
        [Display(Name = "SEM_STATUS", ResourceType = typeof(Resources.RsCadastros))]
        Sem_Status = 5,     
    }
}
