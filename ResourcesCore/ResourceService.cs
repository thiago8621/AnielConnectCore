using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Resources
{
    public static class ResourceService
    {
        public static String GetResource(Type resource, String resourceName)
        {
            return new ResourceManager(resource).GetString(resourceName);
        }

    }
}
