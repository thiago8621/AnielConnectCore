using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Sinapse.DAO
{
    public class SessionFactoryManager
    {
        private static ISessionFactory factory;
        private static object padlock = new Object();
        private static Configuration config;
        private static String arquivoConnection = "Connection.txt";

        public static String StringConnectionAlternativo
        {
            get
            {
                if (!File.Exists(arquivoConnection))
                {
                    return "";
                }
                else
                {
                    StreamReader SR = File.OpenText(arquivoConnection);
                    try
                    {
                        return SR.ReadLine();
                    }
                    finally
                    {
                        SR.Close();
                    }
                }
            }
            set
            {
                DeleteArquivoConnection();

                System.IO.File.WriteAllText(arquivoConnection, value);
            }
        }

        public static void DeleteArquivoConnection()
        {
            if (File.Exists(arquivoConnection))
            {
                File.Delete(arquivoConnection);
            }
        }

        public static void SetSessionFactory(ISessionFactory pFactory)
        {
            factory = pFactory;
        }

        public static ISessionFactory GetSessionFactory()
        {
            lock (padlock)
            {

                if (factory == null)
                {
                    LoadConfiguration();

                    factory = config.BuildSessionFactory();

                    //LoadAuxiliaryDatabaseObject();
                }

                return factory;
            }
        }

        //public static void LoadAuxiliaryDatabaseObject()
        //{
        //    if (System.Configuration.ConfigurationManager.AppSettings["IntegracaoAniel"] == null)
        //    {
        //        return;
        //    }

        //    if (System.Configuration.ConfigurationManager.AppSettings["IntegracaoAniel"] == "N")
        //    {
        //        return;
        //    }

        //    String Assemblys = System.Configuration.ConfigurationManager.AppSettings["AssemblysBusiness"];



        //    foreach (String _assembly in Assemblys.Split(';'))
        //    {
        //        List<AnielAuxiliaryDatabaseObject> listaObjAuxiliares = new List<AnielAuxiliaryDatabaseObject>();
        //        foreach (Type _objTriggger in Assembly.Load(_assembly).GetTypes().ToList().FindAll(p => p.GetInterface("IAuxiliaryDatabaseObject") != null
        //            ).OrderBy(p => p.Name))
        //        {
        //            AnielAuxiliaryDatabaseObject obj = (AnielAuxiliaryDatabaseObject)Activator.CreateInstance(_objTriggger);
        //            listaObjAuxiliares.Add(obj);
        //        }

        //        foreach (AnielAuxiliaryDatabaseObject obj in listaObjAuxiliares.OrderBy(p => p.OrdemExecucao))
        //        {
        //            config.AddAuxiliaryDatabaseObject(obj);
        //        }
        //    }

        //    new SchemaExport(config).Create(true, true);
        //}

        public static void LoadConfiguration()
        {
            config = new Configuration();

            config.Configure();

            if (!String.IsNullOrEmpty(StringConnectionAlternativo))
                config.SetProperty("connection.connection_string", StringConnectionAlternativo);

            if (String.IsNullOrEmpty(config.Properties["connection.connection_string"].Trim()))
            {
                throw new EmptyStringConnectionException("String de conexão não definida");
            }

            try
            {
                String Assemblys = System.Configuration.ConfigurationManager.AppSettings["AssemblysBusiness"];

                foreach (String _assembly in Assemblys.Split(';'))
                {
                    config.AddAssembly(_assembly);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Erro ao carregar assemblys Business ({0})", ex.Message), ex);
            }


        }
    }
}
