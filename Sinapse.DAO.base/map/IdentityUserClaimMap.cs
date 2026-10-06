using FluentNHibernate.Mapping;
using Sinapse.Model.Identity;

namespace Sinapse.DAO.map
{
    public class IdentityUserClaimMap : ClassMap<IdentityUserClaim>
    {
        public IdentityUserClaimMap()
        {
            Table("AspNetUserClaims");
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.ClaimType);
            Map(x => x.ClaimValue);

            HasOne(x => x.User).ForeignKey("UserId");
        }
    }
}
