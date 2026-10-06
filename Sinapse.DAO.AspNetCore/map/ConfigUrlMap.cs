using FluentNHibernate.Mapping;
using Sinapse.Model.AspNetCore;

namespace Aniel.PontoEletronico.DAO.AspNetCore.map
{
    public class ConfigUrlMap : ClassMap<ConfigUrl>
    {
        public ConfigUrlMap() : base()
        {
            Table("TB_CONFIG_URL");

            Id(u => u.Sistema, "SISTEMA");
            Map(x => x.Url, "URL");
        }
    }
}
