using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{

    public enum Enum_VisualizacaoDashboard
    {
        [Display(Name = "TODOS", ResourceType = typeof(Resources.RsBase))]
        TODOS = 0,
        [Display(Name = "VENDEDOR", ResourceType = typeof(Resources.RsCadastros))]
        VENDEDOR = 1,
        [Display(Name = "EQUIPE_", ResourceType = typeof(Resources.RsCadastros))]
        EQUIPE = 2
    }
}