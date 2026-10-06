using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Aniel.Connect.Core.AspNetCore
{
    public class CookieTempDataProvider : ITempDataProvider
    {
        internal const string TempDataCookieKey = "__ControllerTempData";

        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            string cookie = context.Request.Cookies[TempDataCookieKey];

            if (!String.IsNullOrEmpty(cookie))
            {
                return DeserializeTempData(cookie);
            }

            return new Dictionary<string, object>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            bool hasTempData = values != null && values.Count > 0;

            if (!hasTempData)
            {
                context.Response.Cookies.Delete(TempDataCookieKey);
            }
            else
            {
                string cookie = SerializeToBase64EncodedString(values);
                context.Response.Cookies.Append(TempDataCookieKey, cookie);
            }
        }
        public static IDictionary<string, object> DeserializeTempData(string base64EncodedSerializedTempData)
        {
            byte[] bytes = Convert.FromBase64String(base64EncodedSerializedTempData);
            var memStream = new MemoryStream(bytes);
            
            var binFormatter = new BinaryFormatter();
            
            return binFormatter.Deserialize(memStream) as IDictionary<string, object> /*TempDataDictionary : This returns NULL*/;
        }

        public static string SerializeToBase64EncodedString(IDictionary<string, object> values)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Seek(0, SeekOrigin.Begin);

            var binFormatter = new BinaryFormatter();
            binFormatter.Serialize(memStream, values);
            
            memStream.Seek(0, SeekOrigin.Begin);
            
            byte[] bytes = memStream.ToArray();
            
            return Convert.ToBase64String(bytes);
        }
    }
}
