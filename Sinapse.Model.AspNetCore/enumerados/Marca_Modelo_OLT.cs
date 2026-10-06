using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sinapse.Model.AspNetCore.enumerados
{
    public enum Marca_Modelo_OLT
    {
        [Display(Name = "HUAWEI", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Huawei = 0,
        [Display(Name = "FIBERHOME_A_B", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Fiberhome_A_B = 1, //Implementação original
        [Display(Name = "ZTE", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        ZTE = 2,
        [Display(Name = "PARKS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Parks = 3,
        [Display(Name = "NOKIA", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Nokia = 4,
        [Display(Name = "DATACOM", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Datacom = 5,
        [Display(Name = "INTELBRAS", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Intelbras = 6,
        [Display(Name = "VSOL", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        VSOL = 7,
        [Display(Name = "HSGQ", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        HSGQ = 8,
        [Display(Name = "CIANET", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Cianet = 9,
        [Display(Name = "FIBERHOME_A1", ResourceType = typeof(ResourcesCore.AspNetCore.RsCadastros))]
        Fiberhome_A1 = 10 //Nova implementação com sintaxe diferente
    }
}
