using System;
using System.Resources;

namespace ResourcesCore.AspNetCore
{
    public static class ResourceService
    {
        public static String GetResource(Type resource, String resourceName)
        {
            return new ResourceManager(resource).GetString(resourceName);
        }

    }
}
