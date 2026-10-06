using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class Tipo_Elemento_Rede : BaseModel
    {
        public virtual String Codigo_Auxiliar { get; set; }

        public virtual String Descricao { get; set; }
        public virtual Tipo_ElementoIcone Icone { get; set; }
        public virtual String Cor { get; set; }
        public virtual Status_Default Situacao { get; set; }
        public virtual String Fabricante { get; set; }
        public virtual Int64 Capacidade { get; set; }
        public virtual Boolean E_Cabo { get; set; }
        public virtual Boolean E_Caixa { get; set; }
        public virtual Tipo_Cabo Tipo_Cabo { get; set; }
        public virtual Tipo_CaboEnum? Tipo_de_Cabo { get; set; }
        public virtual String Espessura { get; set; }
        public virtual String Espessura_Linha { get; set; }
        public virtual Int32? Num_Pares_Fibra { get; set; }
        public virtual Int32? Num_Tubos_Loose { get; set; }
        public virtual Int32? Total_Tubos_loose { get; set; }

        public virtual String Cor_Linha_Ativa { get; set; }
        public virtual String Cor_Linha_Inativa { get; set; }
        public virtual Estilo_LinhaEnum? Estilo_Linha_Ativa { get; set; }
        public virtual Estilo_LinhaEnum? Estilo_Linha_Inativa { get; set; }
        public virtual Modelo_Elemento_Rede Modelo { get; set; }
        public virtual Padrao_Cor? Padrao { get; set; }
        //public virtual Tipo_ElementoMaterial Material { get; set; }
        //public virtual Tipo_ElementoCaderno Servico { get; set; }


    }
}
