using System;
using System.Collections.Generic;

namespace Aniel.Connect.Core.AspNetCore.Models
{
    public class ReplaceDocXModel
    {
        public virtual String Placeholder { get; set; }
        public virtual String ReplaceValue { get; set; }
    }

    public class PostGerarPdfModel
    {
        public virtual String caminho { get; set; }
        public virtual String caminhooutput { get; set; }
        public virtual String caminhohtml { get; set; }

        public virtual String caminhopdf { get; set; }
        public virtual List<ReplaceDocXModel> lista_replace { get; set; }
        public virtual String filename { get; set; }
    }
}
