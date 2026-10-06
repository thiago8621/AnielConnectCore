using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Atividade_Nf
    {
        [Display(Name = "PLANO_PRODUTO", ResourceType = typeof(Resources.RsCadastros))]
        Plano_Produto = 0,
        [Display(Name = "INSTALACAO", ResourceType = typeof(Resources.RsCadastros))]
        Instalacao = 1
    }
}
