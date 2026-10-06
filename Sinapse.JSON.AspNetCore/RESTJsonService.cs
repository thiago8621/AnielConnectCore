using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Sinapse.JSON.AspNetCore
{
    public class RESTJsonService
    {
        public static T JsonStreamToObject<T>(Stream stream) where T : class
        {
            StreamReader reader = new StreamReader(stream);
            String jsonObject = reader.ReadToEnd();
            T objetoParsed = JsonService.DeserializeJasonMapper<T>(jsonObject);

            return objetoParsed;
        }

        public static T JsonToObject<T>(String obj) where T : class
        {
            T objetoParsed = JsonService.DeserializeJasonMapper<T>(obj);

            return objetoParsed;
        }
    }
}
