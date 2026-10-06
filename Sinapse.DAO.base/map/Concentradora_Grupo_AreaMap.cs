using Sinapse.Model;
using Sinapse.Model.enumerados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.map
{
    public class Concentradora_Grupo_AreaMap : ClassMapBase<Concentradora_Grupo_Area>
    {
        public Concentradora_Grupo_AreaMap() : base()
        {
            Table("TB_CONCENTRADORA_GRUPO_AREA");

            Map(p => p.Cod_Escritorio, "COD_ESCRITORIO");
            Map(p => p.Cod_Area, "COD_AREA");
            Map(p => p.Cod_Localidade, "COD_LOCALIDADE");

            Map(p => p.Desc_Escritorio, "DESC_ESCRITORIO");
            Map(p => p.Desc_Area, "DESC_AREA");
            Map(p => p.Localidade, "DESC_LOCALIDADE");

            Map(p => p.Status, "STATUS").CustomType<Status_Default>();

            References(x => x.Concentradora, "ID_CONCENTRADORA").NotFound.Ignore().Fetch.Join().ForeignKey("FK_CONC_GRUPOAREA_CONCENTRADORA");
        }
    }
}
