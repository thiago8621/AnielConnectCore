using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Padrao
    {
        [Display(Name = "NAO", ResourceType = typeof(Resources.RsCadastros))]
        Nao = 0,
        [Display(Name = "SIM", ResourceType = typeof(Resources.RsCadastros))]
        Sim = 1
    }
}
