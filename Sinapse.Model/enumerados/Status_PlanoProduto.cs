using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Status_PlanoProduto
    {
        [Display(Name = "AGUARDANDO_CRIACAO_OS", ResourceType = typeof(Resources.RsCadastros))]
        Aguardando_Criacao_OS = 0,
        [Display(Name = "AGUARDANDO_INSTALACAO", ResourceType = typeof(Resources.RsCadastros))]
        Aguardando_Instalacao = 1,
        [Display(Name = "HABILITADO", ResourceType = typeof(Resources.RsCadastros))]
        Habilitado = 2,
        [Display(Name = "SUSPENSO_DEBITO", ResourceType = typeof(Resources.RsCadastros))]
        Suspenso_Debito = 3,
        [Display(Name = "SUSPENSO_EMPRESA", ResourceType = typeof(Resources.RsCadastros))]
        Suspenso_Empresa = 4,
        [Display(Name = "CANCELADO", ResourceType = typeof(Resources.RsCadastros))]
        Cancelado = 5,
        [Display(Name = "AGUARDANDO_APROVACAO_VENDA", ResourceType = typeof(Resources.RsCadastros))]
        Aguardando_Aprovacao_Venda = 6,
        [Display(Name = "VENDA_CANCELADA", ResourceType = typeof(Resources.RsCadastros))]
        Venda_Cancelada = 7,
        [Display(Name = "CANCELADO_MUDANCA_PLANO", ResourceType = typeof(Resources.RsCadastros))]
        Cancelado_Mudanca_Plano = 8,        
    }
}
