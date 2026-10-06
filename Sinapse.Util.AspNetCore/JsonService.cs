using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Sinapse.Util.AspNetCore
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

        // A classe Control utilizada no parâmetro não existe no ASP.NET Core e não
        // consegui encontrar uma classe equivalente 

        //public static String GetJasonMapper(Dictionary<String, Control> mapeamento)
        //{
        //    List<JsonMapperObject> jasonMap = new List<JsonMapperObject>();

        //    foreach (String propertyName in mapeamento.Keys)
        //    {
        //        JsonMapperObject jsonMap = new JsonMapperObject() { PropertyName = propertyName, ControlID = mapeamento[propertyName].Content };
        //        jasonMap.Add(jsonMap);
        //    }

        //    return JsonService.SerializeJasonMapper(jasonMap);
        //}
    }
}
