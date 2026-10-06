using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;

namespace Sinapse.Model
{
    public class Elemento_RedeFotosArquivos
    {
        public virtual String Descricao { get; set; }
        public virtual Int64 Id { get; set; }
        public virtual Elemento_Rede Elemento { get; set; }
        public virtual String Tipo_FotoArquivo { get; set; }
        public virtual String Url { get; set; }
    }
}
