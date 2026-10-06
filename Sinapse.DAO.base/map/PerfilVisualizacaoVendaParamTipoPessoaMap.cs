using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class PerfilVisualizacaoVendaParamTipoPessoaMap : ClassMapBase<PerfilVisualizacaoVendaParamTipoPessoa>
    {
        public PerfilVisualizacaoVendaParamTipoPessoaMap() : base()
        {
            Table("TB_PERFIL_VIS_VENDA_TIPO_PES");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PERFIL_VIS_VENDA_TIPO_PES");

            References(x => x.PerfilVisualizacao, "ID_PERFIL_VISUALIZACAO").ForeignKey("FK_PERFIL_VIS_VENDA_TIPO_PES");

            Map(x => x.CodTipoPessoa, "ID_TIPO_PESSOA").CustomType<Tipo_Pessoa>();
        }
    }
}
