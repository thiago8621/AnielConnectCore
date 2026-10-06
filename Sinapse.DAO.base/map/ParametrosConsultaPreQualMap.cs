using Sinapse.Model;
using Sinapse.Model.enumerados;

namespace Sinapse.DAO.map
{
    public class ParametrosConsultaPreQualMap : ClassMapBase<ParametrosConsultaPreQual>
    {
        public ParametrosConsultaPreQualMap() : base()
        {
            Table("TB_PARAM_CONS_CRED_PREQUAL");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_PARAM_CONS_CRED_PREQUAL");

            Map(x => x.Descricao, "DESCRICAO");

            Map(x => x.Padrao, "PADRAO");


            Map(x => x.Status, "STATUS").CustomType<Status_Default>();

            HasMany(x => x.ParamsTipoPessoa)
                .KeyColumn("ID_PARAM_CONS_CRED_PREQUAL")
                .ReadOnly();

            //HasMany(x => x.ParamsMeioCaptacao)
            //    .KeyColumn("ID_PERFIL_VISUALIZACAO")
            //    .ReadOnly();

            HasMany(x => x.ParamsCategoriaCliente)
                .KeyColumn("ID_PARAM_CONS_CRED_PREQUAL")
                .ReadOnly();

            HasMany(x => x.ParamsSetorDepto)
                .KeyColumn("ID_PARAM_CONS_CRED_PREQUAL")
                .ReadOnly();

            HasMany(x => x.ParamsRegiao)
                .KeyColumn("ID_PARAM_CONS_CRED_PREQUAL")
                .ReadOnly();

            //HasMany(x => x.ParamsEquipeVenda)
            //    .KeyColumn("ID_PERFIL_VISUALIZACAO")
            //    .ReadOnly();
        }
    }
}
