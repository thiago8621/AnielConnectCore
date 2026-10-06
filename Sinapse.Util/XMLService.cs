using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Sinapse.Util
{
    public static class XMLService
    {
        public static String GetParametroXml(String pNomeArquivo, String Sessao, String NomeParam = "key", String ValorParam = "Build", String Retorno = "value")
        {
            if (!File.Exists(pNomeArquivo))
            {
                throw new Exception($"Arquivo {pNomeArquivo} não encontrado");
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(pNomeArquivo);

                XmlNodeList appSettings = doc.DocumentElement.SelectNodes(Sessao);

                foreach (XmlNode xmlNode in appSettings)
                    if (xmlNode.Attributes[NomeParam].Value == ValorParam)
                        return xmlNode.Attributes[Retorno].Value;

                return String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }

        public static void WriteInnerHibernateCfg(String pNomeArquivo, Boolean HabilitaAtualizacaoAuto)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pNomeArquivo);

            XmlNodeList appSettings = doc.DocumentElement.SelectNodes("/*/*/*");

            foreach (XmlNode xmlNode in appSettings)
            {
                if (xmlNode.Name == "property")
                {
                    if (xmlNode.Attributes["name"].Value == "hbm2ddl.auto")
                    {
                        xmlNode.InnerText = HabilitaAtualizacaoAuto ? "update" : "none";
                    }
                }
            }
            doc.Save(pNomeArquivo);
        }

        public static void Parse(dynamic parent, XElement node)
        {
            if (node.HasElements)
            {
                if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
                {
                    //list
                    var item = new ExpandoObject();
                    var list = new List<dynamic>();
                    foreach (var element in node.Elements())
                    {
                        Parse(list, element);
                    }

                    AddProperty(item, node.Elements().First().Name.LocalName, list);
                    AddProperty(parent, node.Name.ToString(), item);
                }
                else
                {
                    var item = new ExpandoObject();

                    foreach (var attribute in node.Attributes())
                    {
                        AddProperty(item, attribute.Name.ToString(), attribute.Value.Trim());
                    }

                    //element
                    foreach (var element in node.Elements())
                    {
                        Parse(item, element);
                    }

                    AddProperty(parent, node.Name.ToString(), item);
                }
            }
            else
            {
                AddProperty(parent, node.Name.ToString(), node.Value.Trim());
            }
        }

        private static void AddProperty(dynamic parent, string name, object value)
        {
            if (parent is List<dynamic>)
            {
                (parent as List<dynamic>).Add(value);
            }
            else
            {
                (parent as IDictionary<String, object>)[name] = value;
            }
        }
        

        /// <summary>
        /// [XmlType("mark")]
        /// class Teste()
        /// [XmlElement("School")]
        /// public virtual string School { get; set; }
        /// [XmlArray("marks")]
        /// public virtual List<ArrayXml> objMarkList = new List<ArrayXml>();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pPath"></param>
        /// <returns></returns>
        public static T ConvertToObj<T>(String pPath)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StreamReader(@pPath);
            T obj = (T)deserializer.Deserialize(textReader);
            textReader.Close();

            return obj;
        }

        public static string ExecutarWebMethod(string url, Dictionary<String, String> args)
        {
            var formulario = new StringBuilder();
            foreach (var key in args.Keys)
            {
                if (formulario.Length > 0) formulario.Append("&");
                var pair = String.Format("{0}={1}", key, args[key]);
                formulario.Append(HttpContext.Current.Server.UrlEncode(pair));
            }

            var request = WebRequest.Create(url);
            request.Method = "POST";

            var writer = new StreamWriter(request.GetRequestStream());
            writer.WriteLine(formulario.ToString());
            writer.Flush();

            var reader = new StreamReader(request.GetRequestStream());
            return reader.ReadToEnd();
        }

        public static T ParseXML<T>(String pString) where T : class
        {
            var reader = XmlReader.Create(ToStream(pString), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document });
            return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
        }

        public static Stream ToStream(String pString)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(pString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


    }
}
