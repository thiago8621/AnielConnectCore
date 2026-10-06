using log4net;
using Microsoft.Extensions.Configuration;
using Sinapse.Util.AspNetCore;
using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Sinapse.DAO.AspNetCore
{
    public static class CONSTANTES_CORE
    {
        /// <summary>
        ///     Configuração vinda do appSettings.json da aplicação web principal
        /// </summary>
        public static IConfiguration Configuration;
        public static String Log4Net = "Aniel.Connect.Core";

        public static ILog log4Net
        {
            get
            {
                return LogManager.GetLogger(Log4Net);
            }
        }

        public static HttpContext HttpContext;

        public static String SIGLA_MODULO;

        public static Boolean TrataErro;

        #region Campos preenchidos pela configuração

        public static void PreencherCamposComAppSettings()
        {
            SIGLA_MODULO = Configuration["SIGLA_MODULO"];
            TrataErro = (Configuration["TrataErro"] ?? "S") == "S";
            CaminhoLogServico = Configuration["caminhoLog"]?.ToString();
            AssemblysMapping = Configuration["Assemblies.Map"] != null ? Configuration["Assemblies.Map"].ToString().Split(';') : new String[] { };
            PaginacaoRegistros = Configuration["PaginacaoRegistros"] != null ? Int32.Parse(Convert.ToString(Configuration["PaginacaoRegistros"])) : 30;
            EnableOptimizations = (Configuration["EnableOptimizations"] != null ?
                                                                          Convert.ToString(Configuration["EnableOptimizations"])
                                                                          : "N") == "S";
        }

        public static void PreencherCaminhoFisico()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            
            //CaminhoFisico = Path.GetDirectoryName(path).Replace(@"/bin", "").Replace(@"/Debug", "").Replace(@"/Release", "");
            CaminhoFisico = Path.GetDirectoryName(path);

            // Campos que dependem do caminho físico
            Versao = XMLService.GetParametroXml($@"{CaminhoFisico}/Build.config", "/configuration/appSettings/add", "key", "Build");

            // A leitura do arquivo Empresa.config também depende do caminho físico, portanto
            // só devemos preencher os campos vindos do Empresa.config após o preenchimento do caminho físico
            CHANGELOG = (XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "CHANGELOG") ?? "S") == "S"; //((Configuration["CHANGELOG"] ?? "N") == "S");
         
            Email_De = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_De"); // Convert.ToString(Configuration["Email_De"]);
            Email_Smtp = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Smtp"); // Convert.ToString(Configuration["Email_Smtp"]);
            Email_Senha = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Senha"); // Convert.ToString(Configuration["Email_Senha"]);

            string portaEmail = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "Email_Porta");

            if (!String.IsNullOrEmpty(portaEmail))
            {
                Email_Porta = Convert.ToInt32(portaEmail);
            }

            BancoMongo = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "BancoMongo"); //Configuration["BancoMongo"] ?? throw new Exception("BancoMongo não definido");
            BancoMongoGeograph = XMLService.GetParametroXml(GetEmpresaConfig(), "/configuration/appSettings/add", "key", "BancoMongoGeograph"); //Configuration["BancoMongo"] ?? throw new Exception("BancoMongo não definido");
            
        }

        #endregion

        //public static String CaminhoFisico = Configuration["CaminhoFisico"];
        public static string CaminhoFisico;

        private static String nomeEmpresaConfig;
        public static String GetEmpresaConfig()
        {
            try
            {
                if (String.IsNullOrEmpty(nomeEmpresaConfig))
                    nomeEmpresaConfig = $@"{CaminhoFisico}/Empresa.config";

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

        public static String CaminhoLogServico;

        public static String Versao;

        public static String[] AssemblysMapping;

        public static String URLPortal
        {
            get
            {
                return GetBaseUrl();
            }
        }

        public static Int32 PaginacaoRegistros;

        public static String Parse_AppId = "vfDajwgs9COFUAJpHsPJLmnUdcghiNVu4bQ2axQy";
        public static String Parse_Key = "QJLnqLU5TT21627luchXk6UISjprdIUAhlQqkyiS";

        public static Boolean EnableOptimizations;

        public static String APLICATION_ID_POINTO_NOVO = "AIzaSyB9SndVsoQa3Mkik1noGOCFuwDHUaVdqXw";
        public static String APLICATION_ID_OLA = "AAAAWYX_-gs:APA91bHoyPTW1zrjMOarcCCxz2aO5ZRpE2Tfx2497mcQwxYB_2J134MElBWcIe5RWke95RUc44lTIVuWpONvJYoamWDeV5VoKvL1MIZ7GXe7UkA_BoECG7mn6XOu4dh-G-1D-V4-ML3V";

        public static Boolean CHANGELOG;

        public static String Email_De;
        public static String Email_Smtp;
        public static String Email_Senha;
        
        public static String BancoMongo;
        public static String BancoMongoGeograph;


        public static Int32 Email_Porta;

        public static string GetBaseUrl()
        {
            try
            {
                var request = HttpContext.Request;
                var baseUrl = $@"{request.Scheme}://{request.Host}{request.PathBase}";

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
