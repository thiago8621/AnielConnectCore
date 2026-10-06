using System;

namespace Aniel.T5.Model.nhibernate
{
    public class ContratoAniel
    {
        public virtual String CodCt { get; set; }
        public virtual String Descricao { get; set; }
        public virtual String Localidade { get; set; }
        public virtual String Fornec_Mat_Padrao { get; set; }
        public virtual String TipoSaldo { get; set; }
        public virtual String TipoSaldoAplic { get; set; }

        public virtual String CodCt_Regulador { get; set; }
        public virtual String Projeto_Regulador { get; set; }

        public virtual String Unidade_Custo { get; set; }
        public virtual String Exigir_Mudanca_Status { get; set; }

        public virtual String Bloquear_Aplic_Remov_OS_Aberta { get; set; }
        public virtual String Bloquear_Terminais_OS_Improd { get; set; }
        public virtual String Baixa_Produtiva_Duplicada { get; set; }

        public virtual Int16? CodEmpesa { get; set; }
        public virtual Int16? CodFilial { get; set; }
        public virtual Int32? Fluxo_Mat { get; set; }
    }
}
