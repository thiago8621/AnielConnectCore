using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Tipo_Integracao_SMS
    {
        [Display(Name = "BVTELECOM", ResourceType = typeof(Resources.RsCadastros))]
        BVTelecom = 0,
        [Display(Name = "PONTALTECH", ResourceType = typeof(Resources.RsCadastros))]
        PontalTech = 1,            
        [Display(Name ="BLIP",ResourceType = typeof(Resources.RsCadastros))]
        Blip = 2,

    }
}
