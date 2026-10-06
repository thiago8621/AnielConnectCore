using System;

namespace Aniel.Connect.Core.AspNetCore.Models
{
    public class Coluna_Dinamica<T>
    {
        public Int32 Index_Coluna { get; set; }
        public T Titulo { get; set; }
    }
}
