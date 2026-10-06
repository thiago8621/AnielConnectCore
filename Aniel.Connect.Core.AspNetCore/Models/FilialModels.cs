using System;

namespace Aniel.Connect.Core.AspNetCore.Models
{
    public class FilialUsuarioModel
    {
        public Int64 Id_Filial { get; set; }
        public virtual String Nome_Filial { get; set; }
        public Int64 Id_Empresa { get; set; }
        public virtual String Nome_Empresa { get; set; }
    }
}
