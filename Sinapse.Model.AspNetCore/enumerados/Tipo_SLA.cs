using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Tipo_SLA
    {
        [Display(Name = "DIAS_CORRIDOS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Dias_Corridos = 0,
        [Display(Name = "DIAS_UTEIS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Dias_Uteis = 1
    }
}
