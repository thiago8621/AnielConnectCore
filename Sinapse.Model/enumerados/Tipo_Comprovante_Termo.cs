using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Comprovante_Termo
    {
        [Display(Name = "SELFIE", ResourceType = typeof(Resources.RsCadastros))]
        Selfie = 0,
        [Display(Name = "PROVA_DE_VIDA", ResourceType = typeof(Resources.RsCadastros))]
        ProvaVida = 1
    }
}
