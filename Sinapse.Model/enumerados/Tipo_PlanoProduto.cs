using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_PlanoProduto
    {
        [Display(Name = "PLANO", ResourceType = typeof(Resources.RsCadastros))]
        Plano = 0,
        [Display(Name = "PRODUTO", ResourceType = typeof(Resources.RsCadastros))]
        Produto = 1
    }
}
