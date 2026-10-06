using Aniel.T5.Model.nhibernate;
using FluentNHibernate.Mapping;

namespace Aniel.T5.DAO.map
{
    public class ContratoAnielMap : ClassMap<ContratoAniel>
    {
        public ContratoAnielMap()
        {
            Table("TB_CONTRATO");

            Id(x => x.CodCt, "CODCT").Length(4);
            Map(x => x.Descricao, "DESCRICAO").Length(30);
            Map(x => x.Localidade, "LOCALIDADE").Length(40);
            Map(x => x.Fornec_Mat_Padrao, "FORNEC_MAT_PADRAO").Length(1);
            Map(x => x.TipoSaldo, "TIPOSALDO").Length(10);
            Map(x => x.TipoSaldoAplic, "TIPOSALDOAPLIC").Length(10);

            Map(x => x.CodCt_Regulador, "CODCT_REGULADOR").Length(4);
            Map(x => x.Projeto_Regulador, "PROJETO_REGULADOR").Length(20);

            Map(x => x.Unidade_Custo, "UNIDADE_CUSTO").Length(10);

            Map(x => x.Exigir_Mudanca_Status, "EXIGIR_MUDANCA_STATUS_RDO_OS");
            Map(x => x.Bloquear_Aplic_Remov_OS_Aberta, "BLOQUEAR_APLIC_REMOV_OS_ABERTA");
            Map(x => x.Bloquear_Terminais_OS_Improd, "BLOQUEAR_TERMINAIS_OS_IMPROD");           
            Map(x => x.Baixa_Produtiva_Duplicada, "CRITICA_BAIXA_PROD_DUPLICADA");           

            Map(x => x.CodEmpesa, "CODEMPRESA");
            Map(x => x.CodFilial, "CODFILIAL");
            Map(x => x.Fluxo_Mat, "FLUXO_MAT");
        }
    }
}
