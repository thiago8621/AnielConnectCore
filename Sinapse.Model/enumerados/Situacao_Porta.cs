using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace Sinapse.Model.enumerados
{
    public enum Situacao_Porta
    {
        [Display(Name = "INDISPONIVEL", ResourceType = typeof(Resources.RsCadastros))]
        Indisponivel = 0,
        [Display(Name = "DISPONIVEL", ResourceType = typeof(Resources.RsCadastros))]
        Disponivel = 1,
        [Display(Name = "PREVISTO", ResourceType = typeof(Resources.RsCadastros))]
        Previsto = 2,
        [Display(Name = "CONSTRUIDO", ResourceType = typeof(Resources.RsCadastros))]
        Construido = 3,
        [Display(Name = "RESERVADO", ResourceType = typeof(Resources.RsCadastros))]
        Reservado = 4,
        [Display(Name = "OCUPADO", ResourceType = typeof(Resources.RsCadastros))]
        Ocupado = 5,
    }
}
