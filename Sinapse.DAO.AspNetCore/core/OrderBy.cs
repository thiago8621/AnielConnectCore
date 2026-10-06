using System;

namespace Sinapse.Model.AspNetCore.core
{
    [Serializable]
    public class OrderBy
    {
        public Int32 Id { get; set; }

        public String NomeCampo { get; set; }

        public OrderByIdentity Direction { get; set; }

        public OrderBy()
        {
            Direction = OrderByIdentity.Asc;
        }
    }
}