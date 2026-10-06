using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sinapse.Util.AspNetCore
{
    public static class AttributesService
    {
        public static String GetDisplayName(this Enum value)
        {
            var attr = (DisplayAttribute)value.GetType()
                                        .GetMember(value.ToString())
                                        .First()
                                        .GetCustomAttribute(typeof(DisplayAttribute));
            return attr.Name;
        }
    }
}
