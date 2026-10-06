using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Models
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

        public virtual string orientationPdf { get; set; }
        public virtual string zoomFactorPdf { get; set; }

        public virtual int? customWidth { get; set; }
        public virtual int? customHeight { get; set; }
    }
    public class ConvertToPDFServiceRequestModel : PostGerarPdfModel
    {
        public String LogoVar { get; set; }
        public String FooterVar { get; set; }
        public String FilenameFooter { get; set; }
    }

}
