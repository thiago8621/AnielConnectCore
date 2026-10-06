using System.ComponentModel.DataAnnotations;

namespace Sinapse.Model.enumerados
{
    public enum Estado_Civil
    {
        [Display(Name = "SOLTEIRO", ResourceType = typeof(Resources.RsBase))]
        Solteiro = 0,
        [Display(Name = "CASADO", ResourceType = typeof(Resources.RsBase))]
        Casado = 1,
        [Display(Name = "VIUVO", ResourceType = typeof(Resources.RsBase))]
        Viuvo = 2,
        [Display(Name = "DIVORCIADO", ResourceType = typeof(Resources.RsBase))]
        Divorciado = 3,
        [Display(Name = "UNIAOESTAVEL", ResourceType = typeof(Resources.RsBase))]
        UniaoEstavel = 4
    }
}
