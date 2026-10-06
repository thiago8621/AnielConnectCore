using Sinapse.Model.Identity.DomainModel;

namespace Sinapse.Model.Identity
{
    public class IdentityUserLogin : ValueObject
    {
        public virtual string LoginProvider { get; set; }

        public virtual string ProviderKey { get; set; }

    }
}
