using log4net;
using Sinapse.Util;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;

namespace Sinapse.DAO
{
    public static class CONSTANTES
    {
        public static String Log4Net = "Aniel.Connect.Core";

        public static ILog log4Net
        {
            get
            {
                return LogManager.GetLogger(Log4Net);
            }
        }

        public static String SIGLA_MODULO = ConfigurationManager.AppSettings["SIGLA_MODULO"];

        public static Boolean TrataErro = (ConfigurationManager.AppSettings["TrataErro"] ?? "S") == "S";

        //public static String CaminhoFisico = ConfigurationManager.AppSettings["CaminhoFisico"];
        public static string CaminhoFisico
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path).Replace(@"\bin", "").Replace(@"\Debug", "").Replace(@"\Release", "");
            }
        }

        private static String nomeEmpresaConfig;
        public static String GetEmpresaConfig()
        {
            try
            {
                if (String.IsNullOrEmpty(nomeEmpresaConfig))
                    nomeEmpresaConfig = $@"{CaminhoFisico}\Empresa.config";

                if (!File.Exists(nomeEmpresaConfig))
                {
                    String xml = @"<?xml version='1.0' encoding='utf-8'?><configuration><appSettings></appSettings></configuration>";
                    File.WriteAllText(nomeEmpresaConfig, xml);
                }
            }
            catch (Exception ex)
            {
                log4Net.Error(ex);
            }
            return nomeEmpresaConfig;
        }

        public static String CaminhoLogServico = ConfigurationManager.AppSettings["caminhoLog"]?.ToString();

        public static String Versao = XMLService.GetParametroXml($@"{CaminhoFisico}\Build.config", "/configuration/appSettings/add", "key", "Build");

        public static String[] AssemblysMapping = ConfigurationManager.AppSettings["Assemblys.Map"] != null ? ConfigurationManager.AppSettings["Assemblys.Map"].ToString().Split(';') : new String[] { };

        public static String URLPortal
        {
            get
            {
                return GetBaseUrl();
            }
        }

        public static Int32 PaginacaoRegistros = ConfigurationManager.AppSettings["PaginacaoRegistros"] != null ? Int32.Parse(Convert.ToString(ConfigurationManager.AppSettings["PaginacaoRegistros"])) : 30;

        public static String Parse_AppId = "vfDajwgs9COFUAJpHsPJLmnUdcghiNVu4bQ2axQy";
        public static String Parse_Key = "QJLnqLU5TT21627luchXk6UISjprdIUAhlQqkyiS";

        public static Boolean EnableOptimizations = (ConfigurationManager.AppSettings["EnableOptimizations"] != null ?
                                                                          Convert.ToString(ConfigurationManager.AppSettings["EnableOptimizations"])
                                                                          : "N") == "S";

        public static String APLICATION_ID_POINTO_NOVO = "AIzaSyB9SndVsoQa3Mkik1noGOCFuwDHUaVdqXw";
        public static String APLICATION_ID_OLA = "AAAAWYX_-gs:APA91bHoyPTW1zrjMOarcCCxz2aO5ZRpE2Tfx2497mcQwxYB_2J134MElBWcIe5RWke95RUc44lTIVuWpONvJYoamWDeV5VoKvL1MIZ7GXe7UkA_BoECG7mn6XOu4dh-G-1D-V4-ML3V";

        public static Boolean CHANGELOG = (XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "CHANGELOG") ?? "S") == "S"; //((ConfigurationManager.AppSettings["CHANGELOG"] ?? "N") == "S");

        public static String Email_De = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_De"); // Convert.ToString(ConfigurationManager.AppSettings["Email_De"]);
        public static String Email_Smtp = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Smtp"); // Convert.ToString(ConfigurationManager.AppSettings["Email_Smtp"]);
        public static String Email_Senha = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Senha"); // Convert.ToString(ConfigurationManager.AppSettings["Email_Senha"]);
        public static String Email_Usuario = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Usuario"); // Convert.ToString(ConfigurationManager.AppSettings["Email_Senha"]);
        public static String Email_UsaSsl = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_UsaSsl"); // Convert.ToString(ConfigurationManager.AppSettings["Email_Senha"]);

        public static String BancoMongo = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "BancoMongo"); //ConfigurationManager.AppSettings["BancoMongo"] ?? throw new Exception("BancoMongo não definido");
        public static String BancoMongoGeograph = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "BancoMongoGeograph"); //ConfigurationManager.AppSettings["BancoMongo"] ?? throw new Exception("BancoMongo não definido");

        public static String KeyUrlPreQual = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "KeyUrlPreQual");

        public static String URLROUTEOSRM = ConfigurationManager.AppSettings["URLROUTEOSRM"] != null ? Convert.ToString(ConfigurationManager.AppSettings["URLROUTEOSRM"]) : "https://router.project-osrm.org/table/v1/driving/";

        public static String crmContactOriginCode = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "crmContactOriginCode");
        public static String integratorAlias = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "integratorAlias");
        public static String integrationCode = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "integrationCode");
        public static String secretKeyRadius = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "secretKeyRadius");

        public static String StreetTypeVoalle = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "StreetTypeVoalle");
        public static string UsaRadius = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "UsaRadius");
        #region Credenciais Firebase Cloud Messaging
        /*public static string ProjectIdGoogle_Connect = Convert.ToString(XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "ProjectIdGoogle_Connect"));
        public static string JsonCredencialGoogle_Connect = Convert.ToString(XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "CaminhoJsonCredencialGoogle_Connect"));
        public static string ProjectIdGoogle_Touch = Convert.ToString(XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "ProjectIdGoogle_Touch"));
        public static string JsonCredencialGoogle_Touch = Convert.ToString(XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "CaminhoJsonCredencialGoogle_Touch"));*/
        public static string ProjectIdGoogle_Connect = "anielconnect-4ec81";
        public static string JsonCredencialGoogle_Connect = $@"{CaminhoFisico}\anielconnect-4ec81-firebase-adminsdk-n5jra-4c33219fd8.json";
        public static string ProjectIdGoogle_Touch = "anieltouch-440b6";
        public static string JsonCredencialGoogle_Touch = $@"{CaminhoFisico}\anieltouch-440b6-firebase-adminsdk-0maz9-5492907e1c.json";
        public static string ProjectIdGoogle_Point = "anielpointnovo";
        public static string JsonCredencialGoogle_Point = $@"{CaminhoFisico}\anielpointnovo-firebase-adminsdk-c52x2-f55e8e98cf.json";
        public static string ProjectIdGoogle_Ola = "ola-cliente";
        public static string JsonCredencialGoogle_Ola = $@"{CaminhoFisico}\anielpointnovo-firebase-adminsdk-c52x2-f55e8e98cf.json";
        #endregion

        public static Int32 Email_Porta
        {
            get
            {
                try
                {
                    String temp = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Porta");
                    return Convert.ToInt32(temp);
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static string GetBaseUrl()
        {
            try
            {
                var request = HttpContext.Current.Request;
                var appUrl = HttpRuntime.AppDomainAppVirtualPath;

                var baseUrl = string.Format(@"{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

                return baseUrl;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// Utilizado nos testes unitários
        /// </summary>
        public static String USUARIO_PADRAO = "ADMIN";
    }
}
