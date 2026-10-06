using FluentNHibernate.Mapping;
using Sinapse.Model.Identity;

namespace Sinapse.DAO.map
{
    public class IdentityUserMap : ClassMap<IdentityUser>
    {
        public IdentityUserMap()
        {
            this.Table("AspNetUsers");
            this.Id(x => x.Id).GeneratedBy.UuidHex("D");

            this.Map(x => x.AccessFailedCount);

            this.Map(x => x.Nome);

            this.Map(x => x.Email);

            this.Map(x => x.EmailConfirmed);

            this.Map(x => x.LockoutEnabled);

            this.Map(x => x.LockoutEndDateUtc);

            this.Map(x => x.PasswordHash);

            this.Map(x => x.PhoneNumber);

            this.Map(x => x.PhoneNumberConfirmed);

            this.Map(x => x.TwoFactorEnabled);

            this.Map(x => x.Token);

            this.Map(x => x.UserName).Length(255).Not.Nullable();

            this.Map(x => x.SecurityStamp);

            this.Map(x => x.PerfilNotificacoesEva);

            this.Map(x => x.E_Monitor);
            this.Map(x => x.Id_Perfil_Agente);

            this.Map(x => x.Ativo);
            this.Map(x => x.Tipo_Usuario);
            this.Map(x => x.Secao_Connect);

            this.Map(x => x.Senha_Aniel).Length(40);

            this.Map(x => x.tokenPremier, "TOKEN_PREMIER");

            this.References(x => x.Funcionario, "ID_FUNCIONARIO").ForeignKey("FK_IDENTITY_FUNC");

            this.References(x => x.Perfil_Dash_Connect, "ID_PERFIL_DASH_CONNECT").ForeignKey("FK_IDENTITY_PERFILDASHCON");

            this.References(x => x.Perfil_Dash_EVA, "ID_PERFIL_DASH_EVA").ForeignKey("FK_IDENTITY_PERFILDASHEVA");

            this.References(x => x.PerfilVisualizacaoServico, "ID_PERFIL_VIS_SERV").ForeignKey("FK_IDENTITY_PERFILVISSERV");

            this.References(x => x.PerfilVisualizacaoVenda, "ID_PERFIL_VIS_VENDA").ForeignKey("FK_IDENTITY_PERFILVISVENDA");

            this.References(x => x.Departamento, "ID_DEPARTAMENTO").ForeignKey("FK_IDENTITY_DEPTO");

            this.HasMany(x => x.Claims).KeyColumn("UserId").Cascade.AllDeleteOrphan();

            //this.HasMany(p => p.PerfilUsuario).KeyColumn("ID_USUARIO").Cascade.AllDeleteOrphan();

            this.HasManyToMany(x => x.Logins)
                .Table("AspNetUserLogins")
                .ChildKeyColumn("UserId")
                .Cascade.AllDeleteOrphan().Component(comp =>
                         {
                             comp.Map(p => p.LoginProvider);
                             comp.Map(p => p.ProviderKey);
                         });


            this.HasManyToMany(x => x.Roles).Table("AspNetUserRoles")
                .ParentKeyColumn("RoleId")
                .ChildKeyColumn("UserId");
            this.Map(x => x.Bloqueia_Modelo_Visualizacao);
            this.Map(x => x.Modelo_Unico_Permitido_ID);

        }
    }
}
