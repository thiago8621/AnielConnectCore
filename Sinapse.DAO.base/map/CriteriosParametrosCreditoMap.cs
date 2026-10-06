using Sinapse.DAO.map;
using Sinapse.Model;

namespace Aniel.Compras.DAO.map
{
    public class CriteriosParametrosCreditoMap : ClassMapBase<CriteriosParamCredito>
    {
        public CriteriosParametrosCreditoMap() : base()
        {
            Table("TB_CRITERIO_PARAM_CREDITO");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_CRITERIO_PARAM_CREDITO");

            Map(u => u.Criterio, "CRITERIO");
            Map(u => u.TipoCriterio, "TIPO_CRITERIO");
            Map(u => u.Valor, "VALOR");
            Map(u => u.Operador, "OPERADOR");
            Map(u => u.TipoOperador, "TIPO_OPERADOR");
            Map(u => u.Pontos, "PONTOS");
            Map(u => u.Eliminatorio, "ELIMINATORIO");
            References(u => u.Id_ParametrosConsultaCredito, "ID_PARAM_CONSULTA").ForeignKey("FK_PARAM_CONSULTA");

        }
    }
}
