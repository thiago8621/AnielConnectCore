using NHibernate;
using Sinapse.DAO.AspNetCore.repository;

namespace Sinapse.DAO.AspNetCore.service
{
    public static class HibernateService
    {
        public static ISessionFactory GetSessionFactory()
        {
            return HibernateUtil.GetSessionFactory().Result;
        }

        public static void SetSessionFactory(ISessionFactory pFactory)
        {
            HibernateUtil.SetSessionFactory(pFactory);
        }

        public static ISessionFactory GetSessionFactoryLog()
        {
            return HibernateUtil.GetSessionFactoryLog().Result;
        }

        public static void SetSessionFactoryLog(ISessionFactory pFactory)
        {
            HibernateUtil.SetSessionFactoryLog(pFactory);
        }

        #region Métodos para informar as dlls de mapeamento

        public static void SetAssembliesMap(string assembliesMap)
        {
            HibernateUtil.AssembliesMap = assembliesMap;
        }
        
        public static void SetAssembliesMapLog(string assembliesMapLog)
        {
            HibernateUtil.AssembliesMapLog = assembliesMapLog;
        }
        
        public static void SetAssembliesMapT5(string assembliesMapT5)
        {
            HibernateUtil.AssembliesMapT5 = assembliesMapT5;
        }

        public static void SetAssembliesMapFoto(string assembliesMapFoto)
        {
            HibernateUtil.AssembliesMapFoto = assembliesMapFoto;
        }

        public static void SetAssembliesMapCherry(string assembliesMapCherry)
        {
            HibernateUtil.AssembliesMapCherry = assembliesMapCherry;
        }
        
        public static void SetAssembliesMapPortalDevSinapse(string assembliesMapPortalDevSinapse)
        {
            HibernateUtil.AssembliesMapPortalDevSinapse = assembliesMapPortalDevSinapse;
        }

        #endregion

    }
}
