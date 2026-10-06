using FluentNHibernate.Cfg;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.AspNetCore.repository
{
    public class HibernateUtil
    {

        private static ISessionFactory factory = null;

        private static ISessionFactory factoryLog = null;

        private static ISessionFactory factoryRadius = null;

        private static ISessionFactory factoryT5 = null;

        private static ISessionFactory factoryFoto = null;

        private static ISessionFactory factoryCherry = null;

        private static ISessionFactory factoryPortalDevSinapse = null;

        private static NHibernate.Cfg.Configuration normalConfig = null;

        private static NHibernate.Cfg.Configuration logConfig = null;

        private static NHibernate.Cfg.Configuration radiusConfig = null;

        private static NHibernate.Cfg.Configuration t5Config = null;

        private static NHibernate.Cfg.Configuration fotoConfig = null;

        private static NHibernate.Cfg.Configuration cherryConfig = null;

        private static NHibernate.Cfg.Configuration factoryPortalDevSinapseConfig = null;

        public static string AssembliesMap { get; set; }
        public static string AssembliesMapLog { get; set; }
        public static string AssembliesMapRadius { get; set; }
        public static string AssembliesMapT5 { get; set; }
        public static string AssembliesMapFoto { get; set; }
        public static string AssembliesMapCherry { get; set; }
        public static string AssembliesMapPortalDevSinapse { get; set; }

        public static void DestroySessionFactory()
        {
            if (factory != null)
            {
                factory.Close();
                factory = null;
            }
        }

        public static void DestroySessionFactoryLog()
        {
            if (factoryLog != null)
            {
                factoryLog.Close();
                factoryLog = null;
            }
        }

        public static void DestroySessionFactoryRadius()
        {
            if (factoryRadius != null)
            {
                factoryRadius.Close();
                factoryRadius = null;
            }
        }

        public static void DestroySessionFactoryT5()
        {
            if (factoryT5 != null)
            {
                factoryT5.Close();
                factoryT5 = null;
            }
        }

        public static void DestroySessionFactoryFoto()
        {
            if (factoryFoto != null)
            {
                factoryFoto.Close();
                factoryFoto = null;
            }
        }

        public static void DestroySessionFactoryCherry()
        {
            if (factoryCherry != null)
            {
                factoryCherry.Close();
                factoryCherry = null;
            }
        }

        public static void DestroySessionFactoryPortalDevSinapse()
        {
            if (factoryPortalDevSinapse != null)
            {
                factoryPortalDevSinapse.Close();
                factoryPortalDevSinapse = null;
            }
        }

        public static ISession GetSession()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession session = GetSessionFactory().Result.OpenSession();
            session.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return session;
        }

        public static ISession GetSessionLog()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionLog = GetSessionFactoryLog().Result.OpenSession();
            sessionLog.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionLog;
        }

        public static ISession GetSessionRadius(string ip, string username, string password)
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionRadius = HibernateUtil.GetSessionFactoryRadius(ip, username, password).Result.OpenSession();
            sessionRadius.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionRadius;
        }

        public static ISession GetSessionRadius()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionRadius = GetSessionFactoryRadius().Result.OpenSession();
            sessionRadius.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionRadius;
        }

        public static ISession GetSessionT5()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionT5 = GetSessionFactoryT5().Result.OpenSession();
            sessionT5.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionT5;
        }

        public static ISession GetSessionFoto()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionFoto = GetSessionFactoryFoto().Result.OpenSession();
            sessionFoto.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionFoto;
        }

        public static ISession GetSessionCherry()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionCherry = GetSessionFactoryCherry().Result.OpenSession();
            sessionCherry.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionCherry;
        }

        public static ISession GetSessionPortalDevSinapse()
        {
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            ISession sessionPortalDevSinapse = GetSessionFactoryPortalDevSinapse().Result.OpenSession();
            sessionPortalDevSinapse.FlushMode = FlushMode.Always;
            //System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;

            return sessionPortalDevSinapse;
        }

        public static IStatelessSession GetStatelessSession()
        {
            IStatelessSession session = GetSessionFactory().Result.OpenStatelessSession();

            return session;
        }

        public static IStatelessSession GetStatelessSessionLog()
        {
            IStatelessSession sessionLog = GetSessionFactoryLog().Result.OpenStatelessSession();

            return sessionLog;
        }

        public static IStatelessSession GetStatelessSessionRadius()
        {
            IStatelessSession sessionRadius = GetSessionFactoryRadius().Result.OpenStatelessSession();

            return sessionRadius;
        }

        public static IStatelessSession GetStatelessSessionT5()
        {
            IStatelessSession sessionT5 = GetSessionFactoryT5().Result.OpenStatelessSession();

            return sessionT5;
        }

        public static IStatelessSession GetStatelessSessionFoto()
        {
            IStatelessSession sessionFoto = GetSessionFactoryFoto().Result.OpenStatelessSession();

            return sessionFoto;
        }

        public static IStatelessSession GetStatelessSessionCherry()
        {
            IStatelessSession sessionCherry = GetSessionFactoryCherry().Result.OpenStatelessSession();

            return sessionCherry;
        }

        public static IStatelessSession GetStatelessSessionPortalDevSinapse()
        {
            IStatelessSession sessionPortalDevSinapse = GetSessionFactoryPortalDevSinapse().Result.OpenStatelessSession();

            return sessionPortalDevSinapse;
        }

        public static async Task<ISessionFactory> GetSessionFactory()
        {
            if (factory == null)
            {
                normalConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernate.cfg.xml");

#if DEBUG
                normalConfig.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(normalConfig);

                foreach (string assembly in AssembliesMap.Split(';'))
                {
                    if (!string.IsNullOrEmpty(assembly))
                        fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                }

                factory = fluent.BuildSessionFactory();

                if (CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSession())
                    {
                        await AtualizaMigrations(AssembliesMap, session);
                    }
                }
            }

            return factory;
        }

        public static void SetSessionFactory(ISessionFactory pFactory)
        {
            factory = pFactory;
        }

        public static async Task<ISessionFactory> GetSessionFactoryLog()
        {
            if (factoryLog == null)
            {
                logConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernatelog.cfg.xml");

#if DEBUG
                logConfig.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(logConfig);

                if (!string.IsNullOrEmpty(AssembliesMapLog))
                {
                    foreach (string assembly in AssembliesMapLog.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(AssembliesMapLog))
                            fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                    }
                }

                factoryLog = fluent.BuildSessionFactory();

                if (!string.IsNullOrEmpty(AssembliesMapLog) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionLog())
                    {
                        await AtualizaMigrations(AssembliesMapLog, session);
                    }
                }
            }

            return factoryLog;
        }

        public static void SetSessionFactoryLog(ISessionFactory pFactory)
        {
            factoryLog = pFactory;
        }

        public static async Task<ISessionFactory> GetSessionFactoryRadius(string ip, string username, string password)
        {
            if (factoryRadius == null)
            {
                radiusConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "\\hibernateradius.cfg.xml");

#if DEBUG
                radiusConfig.SetProperty("show_sql", "true");
#endif

                string connection_string = $"Persist Security Info=False;Server={ip};Port=3306;Database=AnielRadius;Uid={username};Pwd={password};IntegratedSecurity=false;AllowBatch=True;";
                radiusConfig.SetProperty("connection.connection_string", connection_string);

                FluentConfiguration fluent = Fluently.Configure(radiusConfig);                

                if (!String.IsNullOrEmpty(AssembliesMapRadius))
                {
                    foreach (String Str in AssembliesMapRadius.Split(';'))
                    {
                        if (!String.IsNullOrEmpty(Str))
                            fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(Str)));
                    }
                }

                factoryRadius = fluent.BuildSessionFactory();

                if (!String.IsNullOrEmpty(AssembliesMapRadius) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionRadius(ip, username, password))
                    {
                        await AtualizaMigrations(AssembliesMapRadius, session);
                    }
                }
            }

            return factoryRadius;
        }

        public static async Task<ISessionFactory> GetSessionFactoryRadius()
        {
            if (factoryRadius == null)
            {
                radiusConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernateradius.cfg.xml");

#if DEBUG
                radiusConfig.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(radiusConfig);

                if (!string.IsNullOrEmpty(AssembliesMapRadius))
                {
                    foreach (string assembly in AssembliesMapRadius.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(assembly))
                            fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                    }
                }

                factoryRadius = fluent.BuildSessionFactory();

                if (!string.IsNullOrEmpty(AssembliesMapRadius) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionRadius())
                    {
                        await AtualizaMigrations(AssembliesMapRadius, session);
                    }
                }
            }

            return factoryRadius;
        }

        public static void SetSessionFactoryRadius(ISessionFactory pFactory)
        {
            factoryRadius = pFactory;
        }

        public static async Task<ISessionFactory> GetSessionFactoryT5()
        {
            if (factoryT5 == null)
            {
                t5Config = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernateT5.cfg.xml");

#if DEBUG
                t5Config.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(t5Config);

                if (!string.IsNullOrEmpty(AssembliesMapT5))
                {
                    foreach (string assembly in AssembliesMapT5.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(assembly))
                            fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                    }
                }

                factoryT5 = fluent.BuildSessionFactory();

                if (!string.IsNullOrEmpty(AssembliesMapT5) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionT5())
                    {
                        await AtualizaMigrations(AssembliesMapT5, session);
                    }
                }
            }

            return factoryT5;
        }

        private static async Task AtualizaMigrations(string Assemblys, ISession session)
        {

            foreach (string _assembly in Assemblys.Split(';'))
            {
                if (!string.IsNullOrEmpty(_assembly))
                {
                    CONSTANTES_CORE.log4Net.Debug($"Atualizando scripts assembly: {_assembly}");
                    List<IAnielMigration> listaSqls = new List<IAnielMigration>();
                    foreach (Type _objTriggger in Assembly.Load(_assembly).GetTypes().ToList().FindAll(p => p.GetInterface("IAnielMigration") != null
                        ).OrderBy(p => p.Name))
                    {
                        AnielMigration obj = (AnielMigration)Activator.CreateInstance(_objTriggger);

                        if (!await ExisteChangeLog(session, obj.Codigo) && _objTriggger != typeof(AnielMigration))
                        {
                            CONSTANTES_CORE.log4Net.Debug($"Atualizando tipo: {_objTriggger.Name}");

                            listaSqls.Add(obj);
                        }
                    }

                    foreach (IAnielMigration obj in listaSqls.OrderBy(p => p.Codigo))
                    {
                        var sql = obj.GetSql();
                        CONSTANTES_CORE.log4Net.Debug($"Executando sql: {sql}");
                        try
                        {
                            if (obj.Tipo == TipoAnielMigration.ScriptMultiplo)
                            {
                                await ExecutaSql(session, sql, ';');
                            }
                            else if (obj.Tipo == TipoAnielMigration.Select)
                            {
                                await SelectSql(session, sql, ';');
                            }
                            else
                            {
                                await ExecutaSql(session, sql);
                            }

                            await AtualizaChangeLog(session, obj.Codigo, obj.Descricao, true, null);
                        }
                        catch (Exception ex)
                        {
                            CONSTANTES_CORE.log4Net.Error($"Migrations: {ex.Message}");
                            await AtualizaChangeLog(session, obj.Codigo, obj.Descricao, false, $"{ex.Message} {ex.InnerException?.Message}");
                        }
                    }
                }
            }
        }

        public static async Task<bool> ExisteChangeLog(ISession session, string Codigo)
        {
            using (var com = session.Connection.CreateCommand())
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>
                {
                    { "@CODIGO", Codigo }
                };

                StringBuilder sql = new StringBuilder("SELECT A.ID FROM TB_CHANGELOG A WHERE A.CODIGO = @CODIGO AND A.EXECUTADO = 1");

                using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                {
                    return await reader.ReadAsync();
                }
            }
        }

        public static async Task AtualizaChangeLog(ISession session, string codigo, string descricao, bool executado, string log)
        {
            using (var com = session.Connection.CreateCommand())
            {
                Dictionary<string, object> parametros = new Dictionary<string, object>
                {
                    { "@CODIGO", codigo },
                    { "@EXECUTADO", executado },
                    { "@DESCRICAO", descricao },
                    { "@LOG", log}
                };

                StringBuilder sql = new StringBuilder("UPDATE OR INSERT INTO TB_CHANGELOG (ID, CODIGO, DESCRICAO, EXECUTADO, LOG, DATAHORACRIACAO)")
                    .AppendLine(" VALUES ((SELECT GEN_ID(GN_CHANGELOG, 1) FROM RDB$DATABASE), @CODIGO, @DESCRICAO, @EXECUTADO, @LOG, CURRENT_TIMESTAMP)")
                    .AppendLine(" MATCHING (CODIGO)");

                await CommandHelper.ExecuteNonQuery(com, sql, parametros, session.Connection, null);
            }
        }

        public static async Task ExecutaSql(ISession session, string sql, char separator = '^')
        {
            using (var com = session.Connection.CreateCommand())
            {
                using (var trans = com.Connection.BeginTransaction())
                {
                    foreach (var _sql in sql.Split(separator))
                    {
                        if (!string.IsNullOrEmpty(_sql.Trim()))
                            await CommandHelper.ExecuteNonQuery(com, new StringBuilder(_sql), new Dictionary<string, object>(), session.Connection, trans);
                    }
                    trans.Commit();
                }
            }
        }

        public static async Task SelectSql(ISession session, string sql, char separator = '^')
        {
            using (var com = session.Connection.CreateCommand())
            {
                using (var trans = com.Connection.BeginTransaction())
                {
                    foreach (var _sql in sql.Split(separator))
                    {
                        if (!string.IsNullOrEmpty(_sql.Trim()))
                        {
                            var dr = await CommandHelper.ExecuteReader(com, new StringBuilder(_sql), new Dictionary<string, object>(), session.Connection, trans);

                            while (dr.Read()) { }
                        }
                    }
                    trans.Commit();
                }
            }
        }

        public static void SetSessionFactoryT5(ISessionFactory pFactory)
        {
            factoryT5 = pFactory;
        }

        public static async Task<ISessionFactory> GetSessionFactoryFoto()
        {
            if (factoryFoto == null)
            {
                fotoConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernateFoto.cfg.xml");

#if DEBUG
                fotoConfig.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(fotoConfig);

                if (!string.IsNullOrEmpty(AssembliesMapFoto))
                {
                    foreach (string assembly in AssembliesMapFoto.Split(';'))
                    {
                        fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                    }
                }

                factoryFoto = fluent.BuildSessionFactory();

                if (!string.IsNullOrEmpty(AssembliesMapFoto) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionFoto())
                    {
                        await AtualizaMigrations(AssembliesMapFoto, session);
                    }
                }

            }

            return factoryFoto;
        }

        public static void SetSessionFactoryFoto(ISessionFactory pFactory)
        {
            factoryFoto = pFactory;
        }


        public static async Task<ISessionFactory> GetSessionFactoryCherry()
        {
            if (factoryCherry == null)
            {
                cherryConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernateCherry.cfg.xml");

#if DEBUG
                cherryConfig.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(cherryConfig);

                if (!string.IsNullOrEmpty(AssembliesMapCherry))
                {
                    foreach (string assembly in AssembliesMapCherry.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(assembly))
                            fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                    }
                }

                factoryCherry = fluent.BuildSessionFactory();

                if (!string.IsNullOrEmpty(AssembliesMapCherry) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionCherry())
                    {
                        await AtualizaMigrations(AssembliesMapCherry, session);
                    }
                }
            }

            return factoryCherry;
        }

        public static void SetSessionFactoryCherry(ISessionFactory pFactory)
        {
            factoryCherry = pFactory;
        }



        public static async Task<ISessionFactory> GetSessionFactoryPortalDevSinapse()
        {
            if (factoryPortalDevSinapse == null)
            {
                cherryConfig = new NHibernate.Cfg.Configuration().Configure(CONSTANTES_CORE.CaminhoFisico + "/hibernatePortalDevSinapse.cfg.xml");

#if DEBUG
                cherryConfig.SetProperty("show_sql", "true");
#endif

                FluentConfiguration fluent = Fluently.Configure(cherryConfig);

                if (!string.IsNullOrEmpty(AssembliesMapPortalDevSinapse))
                {
                    foreach (string assembly in AssembliesMapPortalDevSinapse.Split(';'))
                    {
                        if (!string.IsNullOrEmpty(assembly))
                            fluent.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.Load(assembly)));
                    }
                }

                factoryCherry = fluent.BuildSessionFactory();

                if (!string.IsNullOrEmpty(AssembliesMapPortalDevSinapse) && CONSTANTES_CORE.CHANGELOG)
                {
                    using (var session = GetSessionCherry())
                    {
                        await AtualizaMigrations(AssembliesMapPortalDevSinapse, session);
                    }
                }
            }

            return factoryCherry;
        }
        public static void SetSessionFactoryPortalDevSinapse(ISessionFactory pFactory)
        {
            factoryPortalDevSinapse = pFactory;
        }


    }
}
