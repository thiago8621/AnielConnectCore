using FluentNHibernate.Mapping;
using Sinapse.Model.AspNetCore;

namespace Sinapse.DAO.AspNetCore.map
{
    public class UsuarioT5Map : ClassMap<UsuarioT5>
    {
        public UsuarioT5Map()
        {
            Table("TB_SENHA");
            Id(x => x.Usuario, "USUARIO");
            Map(x => x.Nome, "NOME");
            Map(x => x.Senha, "SENHA");
            Map(x => x.Ativo, "ATIVO");
            Map(x => x.Administrador, "ADMINISTRADOR");
            Map(x => x.Perfil_Dash_Cherry, "PERFIL_DASH_CHERRY");
            Map(x => x.Desc_Perfil_Dash_Cherry, "DESC_PERFIL_DASH_CHERRY");
            References(x => x.Funcionario, "NUM_FRE").ForeignKey("FK_SENHA_FUNC");
        }
    }
}
