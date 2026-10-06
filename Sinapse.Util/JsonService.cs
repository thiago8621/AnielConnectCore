using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Sinapse.Util
{
    public class JsonService
    {
        public static String SerializeJasonMapper(List<JsonMapperObject> jasonMap)
        {
            String jSonText = JsonConvert.SerializeObject(jasonMap);

            return jSonText;
        }

        public static String SerializeJasonMapper(List<JsonMapperJQuery> jasonMap)
        {
            String jSonText = JsonConvert.SerializeObject(jasonMap);

            return jSonText;
        }

        public static String SerializeObject(object objeto)
        {
            String jSonText = JsonConvert.SerializeObject(objeto);

            return jSonText;
        }

        public static T DeserializeObject<T>(String jasonMap) where T : new()
        {
            T jSonObject = JsonConvert.DeserializeObject<T>(jasonMap);

            return jSonObject;
        }

        public static String SerializeJasonMapper(object obj)
        {
            String jSonText = JsonConvert.SerializeObject(obj);

            return jSonText;
        }

        public static T DeserializeJasonMapper<T>(String jasonMap) where T : class
        {
            T jSonObject = JsonConvert.DeserializeObject<T>(jasonMap);

            return jSonObject;
        }

        public static String GetJasonMapper(Dictionary<String, Control> mapeamento)
        {
            List<JsonMapperObject> jasonMap = new List<JsonMapperObject>();

            foreach (String propertyName in mapeamento.Keys)
            {
                JsonMapperObject jsonMap = new JsonMapperObject() { PropertyName = propertyName, ControlID = mapeamento[propertyName].ClientID };
                jasonMap.Add(jsonMap);
            }

            return JsonService.SerializeJasonMapper(jasonMap);
        }
    }
}
