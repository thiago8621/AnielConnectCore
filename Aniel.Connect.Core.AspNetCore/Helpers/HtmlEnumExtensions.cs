using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using ResourcesCore.AspNetCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Aniel.Connect.Core.AspNetCore.Helpers
{
    public static class HtmlEnumExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="helper"></param>
        /// <returns>Retorna um dicionario com par valor de dicionario</returns>
        public static StringHtmlContent EnumToString<T>(this HtmlHelper helper) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<int>();
            var enumDictionary = values.ToDictionary(value => GetEnumDescription<T>((T)Enum.Parse(typeof(T), value.ToString())));

            return new StringHtmlContent(JsonConvert.SerializeObject(enumDictionary));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="helper"></param>
        /// <returns>Retorna array de descrição de enum</returns>
        public static StringHtmlContent GetArrayEnum<T>(this HtmlHelper helper) where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<int>();

            var enumDictionary = values.Select(value => GetEnumDescription<T>((T)Enum.Parse(typeof(T), value.ToString())));

            return new StringHtmlContent(JsonConvert.SerializeObject(enumDictionary));
        }

        public static string GetEnumDescription<T>(T value) where T : Enum
        {
            if (value == null)
                return null;

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DisplayAttribute[] attributes =
                (DisplayAttribute[])fi.GetCustomAttributes(
                typeof(DisplayAttribute),
                false);

            try
            {
                if (attributes != null &&
                    attributes.Length > 0)
                {
                    return ResourceService.GetResource(attributes[0].ResourceType, attributes[0].Name);
                }
                else
                    return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        public static string GetEnumDescription(object value)
        {
            if (value == null)
                return null;

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DisplayAttribute[] attributes =
                (DisplayAttribute[])fi.GetCustomAttributes(
                typeof(DisplayAttribute),
                false);

            try
            {
                if (attributes != null &&
                    attributes.Length > 0)
                {
                    return ResourceService.GetResource(attributes[0].ResourceType, attributes[0].Name);
                }
                else
                    return value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }


        public static string GetEnumDescription<T>(String value) where T : Enum
        {
            try
            {
                if (String.IsNullOrEmpty(value)) return String.Empty;

                T param = (T)Enum.Parse(typeof(T), value);

                return GetEnumDescription<T>(param);
            }
            catch
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Retorna a Sql com os enumerados no case
        /// Ex.: getEnumDescriptionInSql("A.STATUS", "STATUS_ORDENAR");
        /// </summary>
        /// <param name="ColunaSql"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static String getEnumDescriptionInSql<T>(String ColunaSql, String Alias) where T : Enum
        {
            StringBuilder builder = new StringBuilder();
            var values = Enum.GetValues(typeof(T));

            builder.AppendLine("CASE");
            for (int i = 0; i < values.Length; i++)
            {
                var texto = GetEnumDescription<T>((T)values.GetValue(i));
                builder.AppendLine($"WHEN {ColunaSql} = {Convert.ToInt32(values.GetValue(i))} THEN '{texto}'");
            }

            builder.AppendLine($"END AS {Alias}");
            return builder.ToString();
        }
    }
}
