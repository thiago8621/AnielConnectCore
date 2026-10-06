namespace Aniel.Connect.Core.AspNetCore.Config.Session
{
    public enum SessionType
    {
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco do Connect
        /// </summary>
        Connect,
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco do Aniel
        /// </summary>
        T5,
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco de log (CONNECTLOG)
        /// </summary>
        Log,
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco de fotos
        /// </summary>
        Foto,
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco do Radius
        /// </summary>
        Radius,
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco do Cherry
        /// </summary>
        Cherry,
        /// <summary>
        ///     Tipo de ISession que se conecta a um banco do PortalDevSinapse
        /// </summary>
        PortalDevSinapse
    }
}
