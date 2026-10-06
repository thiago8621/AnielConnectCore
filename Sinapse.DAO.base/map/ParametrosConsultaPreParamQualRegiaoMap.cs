using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class ParametrosConsultaPreParamQualRegiaoMap : ClassMapBase<ParametrosConsultaPreQualParamRegiao>
    {
        public ParametrosConsultaPreParamQualRegiaoMap() : base()
        {
            Table("TB_PARAM_CONS_CRED_REG");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PARAM_CONS_CRED_REG");

            References(x => x.ParametrosConsultaPreQual, "ID_PARAM_CONS_CRED_PREQUAL").ForeignKey("FK_PARAM_PREQUAL_REG");

            Map(x => x.CodUF, "ID_UF").Index("IDX_PARAM_CONS_CRED_REG_UF");

            Map(x => x.CodCidade, "ID_CIDADE").Index("IDX_PARAM_CONS_CRED_REG_CID");

            Map(x => x.CodBairro, "ID_BAIRRO").Index("IDX_PARAM_CONS_CRED_REG_BAIR");

            Map(x => x.TodasRegioes, "TODAS_REGIOES");
        }
    }
}
