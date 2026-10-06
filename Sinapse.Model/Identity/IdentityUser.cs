using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNet.Identity;
using Sinapse.Model.Identity.DomainModel;
using Sinapse.Model.nhibernate;
using Sinapse.ModelLog;
using Resources;
using Aniel.Connect.ModelLog.nhibernate;

namespace Sinapse.Model.Identity
{
    public class IdentityUser : EntityWithTypedId<String>, IUser
    {
      
        public virtual bool Bloqueia_Modelo_Visualizacao { get; set; }

      
        public virtual int? Modelo_Unico_Permitido_ID { get; set; }
        public virtual int AccessFailedCount { get; set; }
        public virtual string Nome { get; set; }
        public virtual string Email { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual DateTime? LockoutEndDateUtc { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }
        public virtual string UserName { get; set; }
        public virtual string SecurityStamp { get; set; }
        public virtual string Token { get; set; }
        public virtual Int64 PerfilNotificacoesEva { get; set; }
        public virtual String Senha_Aniel { get; set; }
        public virtual Boolean E_Monitor { get; set; }
        public virtual Int64? Id_Perfil_Agente { get; set; }
        public virtual Int64? Secao_Connect { get; set; }
        public virtual Int64? Tipo_Usuario { get; set; }
        public virtual Departamento Departamento { get; set; }
        public virtual Funcionario Funcionario { get; set; }      
        public virtual Perfil_DashBoard_Connect Perfil_Dash_Connect { get; set; }
        public virtual Perfil_DashBoard_Connect Perfil_Dash_EVA { get; set; }
        public virtual PerfilVisualizacaoServico PerfilVisualizacaoServico { get; set; }
        public virtual PerfilVisualizacaoVenda PerfilVisualizacaoVenda { get; set; }
        public virtual ParametrosConsultaPreQual ParametrosConsultaPreQual { get; set; }

        public virtual ICollection<IdentityRole> Roles { get; protected set; }
        public virtual ICollection<IdentityUserClaim> Claims { get; protected set; }
        public virtual ICollection<IdentityUserLogin> Logins { get; protected set; }
        public virtual String tokenPremier { get; set; }
        public virtual Boolean Ativo { get; set; }
        public virtual Boolean isSenhaMaster { get; set; }

        public IdentityUser()
        {
            this.Roles = new List<IdentityRole>();
            this.Claims = new List<IdentityUserClaim>();
            this.Logins = new List<IdentityUserLogin>();
        }

        public IdentityUser(string userName)
            : this()
        {
            this.UserName = userName;
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        public static string GetEnumDescription(object value)
        {
            if (value == null)
                return null;

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DisplayAttribute[] attributes =
                (DisplayAttribute[])fi.GetCustomAttributes(
                typeof(DisplayAttribute),
                false);

            try
            {
                if (attributes != null &&
                    attributes.Length > 0)
                {
                    return ResourceService.GetResource(attributes[0].ResourceType, attributes[0].Name);
                }
                else
                    return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        public virtual void Log_InsereTipoOper<T, TLog>(List<TLog> logs, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog, String pChave)
           
           where TLog : LogGenerico<Int64>, new()
        {
            var log = new TLog();
            logs.Add((TLog)log.Bind(Convert.ToInt32(this.Id), pUsuario, pTipoOperLog, pChave, GetEnumDescription(pTipoOperLog), String.Empty, String.Empty, pNomeUsuario));
        }


        public virtual List<LogVincularPerfilBase> GeraLog(IdentityUser pOld, String pUsuario, String pNomeUsuario, TipoOperLog pTipoOperLog)
        {
            List<LogVincularPerfilBase> logs = new List<LogVincularPerfilBase>();

            if (pTipoOperLog != TipoOperLog.Alteracao)
            {
                Log_InsereTipoOper<IdentityUser, LogVincularPerfilBase>(logs, pUsuario, pNomeUsuario, pTipoOperLog, this.Id.ToString());
            }
            //else
            //{
            //    Log_ComparaProperty<IdentityUser, LogVincularPerfilAniel>(logs, pUsuario, pNomeUsuario, pTipoOperLog, this.Id.ToString(), RsCadastros.USUARIO, pOld, this, p => p.UserName);
            //    Log_ComparaProperty<IdentityUser, LogVincularPerfilAniel>(logs, pUsuario, pNomeUsuario, pTipoOperLog, this.Id.ToString(), RsCadastros.ATIVO, pOld, this, p => p.Ativo);

            //}

            return logs;

        }
    }


    //public class IdentityUserMap : ClassMapping<IdentityUser>
    //{
    //    public IdentityUserMap()
    //    {
    //        this.Table("AspNetUsers");
    //        this.Id(x => x.Id, m => m.Generator(new UUIDHexCombGeneratorDef("D")));

    //        this.Property(x => x.AccessFailedCount);

    //        this.Property(x => x.Email);

    //        this.Property(x => x.EmailConfirmed);

    //        this.Property(x => x.LockoutEnabled);

    //        this.Property(x => x.LockoutEndDateUtc);

    //        this.Property(x => x.PasswordHash);

    //        this.Property(x => x.PhoneNumber);

    //        this.Property(x => x.PhoneNumberConfirmed);

    //        this.Property(x => x.TwoFactorEnabled);

    //        this.Property(x => x.Token);

    //        this.Property(x => x.UserName, map =>
    //        {
    //            map.Length(255);
    //            map.NotNullable(true);
    //            map.Unique(true);
    //        });

    //        this.Property(x => x.SecurityStamp);

    //        this.Bag(x => x.Claims, map =>
    //        {
    //            map.Key(k =>
    //            {
    //                k.Column("UserId");
    //                k.Update(false); // to prevent extra update afer insert
    //            });
    //            map.Cascade(Cascade.All | Cascade.DeleteOrphans);
    //        }, rel =>
    //        {
    //            rel.OneToMany();
    //        });

    //        this.Set(x => x.Logins, cam =>
    //        {
    //            cam.Table("AspNetUserLogins");
    //            cam.Key(km => km.Column("UserId"));
    //            cam.Cascade(Cascade.All | Cascade.DeleteOrphans);
    //        },
    //                 map =>
    //                 {
    //                     map.Component(comp =>
    //                     {
    //                         comp.Property(p => p.LoginProvider);
    //                         comp.Property(p => p.ProviderKey);
    //                     });
    //                 });

    //        this.Bag(x => x.Roles, map =>
    //        {
    //            map.Table("AspNetUserRoles");
    //            map.Key(k => k.Column("UserId"));
    //        }, rel => rel.ManyToMany(p => p.Column("RoleId")));
    //    }
    //}
}