using Sinapse.Model.AspNetCore;
using Sinapse.Model.AspNetCore.enumerados;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sinapse.DAO.AspNetCore.map
{
    public class OLT_Grupo_AreaMap : ClassMapBase<OLT_Grupo_Area>
    {
        public OLT_Grupo_AreaMap() : base()
        {
            Table("TB_OLT_GRUPO_AREA");

            Id(u => u.Id, "ID").GeneratedBy.Sequence("GN_OLT_GRUPO_AREA");

            Map(p => p.Cod_Escritorio, "COD_ESCRITORIO").Length(4);
            Map(p => p.Cod_Area, "COD_AREA").Length(4);
            Map(p => p.Desc_Escritorio, "DESC_ESCRITORIO");
            Map(p => p.Desc_Area, "DESC_AREA");

            Map(p => p.Status, "STATUS").CustomType<Status_Default>();

            References(x => x.OLT, "ID_OLT").NotFound.Ignore().Fetch.Join().ForeignKey("FK_OLT_GRUPOAREA_OLT");
        }

    }
}
