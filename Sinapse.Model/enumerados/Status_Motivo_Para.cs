using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Status_Motivo_Para
    {
        [Display(Name = "PLANO_PRODUTO_CONTRATO", ResourceType = typeof(Resources.RsCadastros))]
        Plano_Produto_Contrato = 1,
        [Display(Name = "PLANO_PRODUTO", ResourceType = typeof(Resources.RsCadastros))]
        Plano_Produto = 2,
        [Display(Name = "CONTRATO", ResourceType = typeof(Resources.RsCadastros))]
        Contrato = 3
    }
}
