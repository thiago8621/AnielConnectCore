using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class ParametrosConsultaPreQualParamTipoPessoaMap : ClassMapBase<ParametrosConsultaPreQualParamTipoPessoa>
    {
        public ParametrosConsultaPreQualParamTipoPessoaMap() : base()
        {
            Table("TB_PARAM_CONS_CRED_TIPO_PES");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PARAM_CONS_CRED_TIPO_PES");

            References(x => x.ParametrosConsultaPreQual, "ID_PARAM_CONS_CRED_PREQUAL").ForeignKey("FK_PARAM_PREQUAL_TIPO_PES");

            Map(x => x.CodTipoPessoa, "ID_TIPO_PESSOA").CustomType<Tipo_Pessoa>();
        }
    }
}
