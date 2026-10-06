using FluentNHibernate.Mapping;
using Sinapse.DAO.map;
using Sinapse.Model;

namespace Sinapse.T5.DAO.Map
{
    public class Elemento_Rede_ArquivoAnielMap : ClassMap<Elemento_RedeFotosArquivos>
    {
        public Elemento_Rede_ArquivoAnielMap()
        {
            Table("TB_ELEMENTO_REDE_ARQUIVO");
            Id(p => p.Id, "ID").GeneratedBy.Sequence("GN_ELEM_ARQUIVO");

            Map(p => p.Descricao, "DESCRICAO").Length(100);
            References(p => p.Elemento, "ID_ELEMENTO").ForeignKey("FK_EL_REDE_FT_ARQ_ID_EL");
            Map(p => p.Tipo_FotoArquivo, "TIPO_ARQUIVO").Length(10);
            Map(p => p.Url, "URL").Length(255);
        }
    }
}
