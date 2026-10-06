using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.Model.enumerados
{
    public enum Marca_Modelo_OLT
    {
        [Display(Name = "HUAWEI", ResourceType = typeof(Resources.RsCadastros))]
        Huawei = 0,
        [Display(Name = "FIBERHOME_A_B", ResourceType = typeof(Resources.RsCadastros))]
        Fiberhome_A_B = 1, //Implementação original
        [Display(Name = "ZTE", ResourceType = typeof(Resources.RsCadastros))]
        ZTE = 2,
        [Display(Name = "PARKS", ResourceType = typeof(Resources.RsCadastros))]
        Parks = 3,
        [Display(Name = "NOKIA", ResourceType = typeof(Resources.RsCadastros))]
        Nokia = 4,
        [Display(Name = "DATACOM", ResourceType = typeof(Resources.RsCadastros))]
        Datacom = 5,
        [Display(Name = "INTELBRAS", ResourceType = typeof(Resources.RsCadastros))]
        Intelbras = 6,
        [Display(Name = "VSOL", ResourceType = typeof(Resources.RsCadastros))]
        VSOL = 7,
        [Display(Name = "HSGQ", ResourceType = typeof(Resources.RsCadastros))]
        HSGQ = 8,
        [Display(Name = "CIANET", ResourceType = typeof(Resources.RsCadastros))]
        Cianet = 9,
        [Display(Name = "FIBERHOME_A1", ResourceType = typeof(Resources.RsCadastros))]
        Fiberhome_A1 = 10 //Nova implementação com sintaxe diferente
    }
}
