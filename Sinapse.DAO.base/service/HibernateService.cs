using NHibernate;
using Sinapse.DAO.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.DAO.service
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

    }
}
