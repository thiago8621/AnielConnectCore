using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class ParametrosConsultaPreQualParamCategoriaClienteMap : ClassMapBase<ParametrosConsultaPreQualParamCategoriaCliente>
    {
        public ParametrosConsultaPreQualParamCategoriaClienteMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_CAT_CLI");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PARAM_CONS_CRED_CAT_CLI");

            References(x => x.ParametrosConsultaPreQual, "ID_PARAM_CONS_CRED_PREQUAL").ForeignKey("FK_PARAM_PREQUAL_CAT_CLI");

            Map(x => x.CodCategoriaCliente, "ID_CATEGORIA_CLIENTE").Index("IDX_PARAM_PREQUAL_CAT_CLI");
        }
    }
}
