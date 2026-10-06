using Sinapse.DAO;

namespace Aniel.Connect
{
    public static class SiteKeys
    {
        public static string StyleVersion
        {
            get
            {
                return "<link href=\"{0}?v=" + CONSTANTES.Versao + "\" rel=\"stylesheet\"/>";
            }
        }
        public static string ScriptVersion
        {
            get
            {
                return "<script src=\"{0}?v=" + CONSTANTES.Versao + "\"></script>";
            }
        }
    }
}
