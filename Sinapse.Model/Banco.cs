using Sinapse.Model.enumerados;
using Sinapse.Model.nhibernate;
using System;

namespace Sinapse.Model
{
    public class Banco : BaseModel
    {
        public virtual String Codigo { get; set; }
        public virtual String Descricao { get; set; }
        public virtual Status_Default Status { get; set; }
        public virtual String Carteira { get; set; }
        public virtual String ConvenioPadraoCNAB { get; set; }
        public virtual String ConvenioCarteiraCodigo { get; set; }
        public virtual bool TransmissaoInstantania { get; set; }
        public virtual String ConvenioAPILD { get; set; }
        public virtual String ConvenioAPIKEY { get; set; }
        public virtual String ConvenioAPISecret { get; set; }
        public virtual String CampoEstacao { get; set; }
        public virtual bool PagamentoAutomatico { get; set; }
        public virtual String Chave { get; set; }
        public virtual String ClientID { get; set; }
        public virtual String ClientSecret { get; set; }
        public virtual String NomeCertificado { get; set; }
        public virtual String Senha { get; set; }
        public virtual String Email { get; set; }
        public virtual String NomeCertificadoCP { get; set; }
        public virtual String SenhaCP { get; set; }
        public virtual String EmailCP { get; set; }
        public virtual String CodigoISPB { get; set; }
        public virtual String CodigoCOMPE { get; set; }


        public virtual Int64? Nosso_Numero_Max{ get; set; }

        public override string ToString()
        {
            return Descricao;
        }
    }
}
