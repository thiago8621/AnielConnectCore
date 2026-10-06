using NHibernate;
using Sinapse.DAO.AspNetCore.repository;

namespace Aniel.Connect.Core.AspNetCore.Config.Session
{
    public static class SessionResolver
    {
        public static ISession Resolve(SessionType type)
        {
            switch (type)
            {
                case SessionType.Connect:
                    return HibernateUtil.GetSession();
                case SessionType.T5:
                    return HibernateUtil.GetSessionT5();
                case SessionType.Log:
                    return HibernateUtil.GetSessionLog();
                case SessionType.Foto:
                    return HibernateUtil.GetSessionFoto();
                case SessionType.Radius:
                    return HibernateUtil.GetSessionRadius();
                case SessionType.Cherry:
                    return HibernateUtil.GetSessionCherry();
                case SessionType.PortalDevSinapse:
                    return HibernateUtil.GetSessionPortalDevSinapse();
                default:
                    return null;
            }
        }
    }
}
