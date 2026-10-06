using Sinapse.Model.Identity;

namespace Sinapse.DAO.map
{
    public class IdentityRoleMap : FluentNHibernate.Mapping.ClassMap<IdentityRole>
    {
        public IdentityRoleMap()
        {
            this.Table("AspNetRoles");
            this.Id(x => x.Id).GeneratedBy.UuidHex("D");
            this.Map(x => x.Name).Length(255).Unique().Not.Nullable();

            this.HasManyToMany(x => x.Users).Table("AspNetUserRoles")
                .ChildKeyColumn("UserId")
                .ParentKeyColumn("RoleId")
                .Cascade.None();
        }
    }
}
