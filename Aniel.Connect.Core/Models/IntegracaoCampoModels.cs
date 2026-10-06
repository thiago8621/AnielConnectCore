using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Models
{
    public class IntegracaoCampo
    {
        public String PropName { get; set; }
        public String Descricao { get; set; }
        public Boolean Requirido { get; set; }
    }

    public class IntegracaoTipoObjeto
    {
        public Type TipoControle { get; set; }
        
        public MethodInfo MetodoControle { get; set; }
        
        public String NomeObjeto { get; set; }
        
        public String TipoViewModel { get; set; }
    }

    public enum IntegracaoTipoStatus
    {
        ProntoParaImportar = 1,
        Importado = 2,
        ErroNaImportacao = 3
    }

    public class IntegracaoStatusImportacao 
    {
        public Int32 Linha {  get; set; }
        public String Mensagem { get; set; }
        public IntegracaoTipoStatus Status { get; set; }
    }
}
