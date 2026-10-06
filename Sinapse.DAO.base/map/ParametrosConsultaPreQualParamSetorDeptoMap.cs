using Sinapse.Model;

namespace Sinapse.DAO.map
{
    public class ParametrosConsultaPreQualParamSetorDeptoMap : ClassMapBase<ParametrosConsultaPreQualParamSetorDepto>
    {
        public ParametrosConsultaPreQualParamSetorDeptoMap() : base()
        {
            Table("TB_PARAM_CONS_CRED_SETOR");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PARAM_CONS_CRED_SETOR");

            References(x => x.ParametrosConsultaPreQual, "ID_PARAM_CONS_CRED_PREQUAL").ForeignKey("FK_PARAM_PREQUAL_SETOR");

            Map(x => x.CodDepto, "ID_DEPTO").Index("IDX_PARAM_PREQUAL_DEPTO");
        }
    }
}
