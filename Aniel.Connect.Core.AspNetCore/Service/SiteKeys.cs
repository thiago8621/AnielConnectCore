using Sinapse.DAO.AspNetCore;

namespace Aniel.Connect.AspNetCore
{
    public static class SiteKeys
    {
        public static string StyleVersion
        {
            get
            {
                return "<link href=\"{0}?v=" + CONSTANTES_CORE.Versao + "\" rel=\"stylesheet\"/>";
            }
        }
        public static string ScriptVersion
        {
            get
            {
                return "<script src=\"{0}?v=" + CONSTANTES_CORE.Versao + "\"></script>";
            }
        }
    }
}
