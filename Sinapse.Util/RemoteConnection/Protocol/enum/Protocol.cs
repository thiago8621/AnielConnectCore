using System.ComponentModel.DataAnnotations;

namespace Sinapse.Util.RemoteConnection
{
    public enum Protocol
    {
        [Display(Name = "SSH", ResourceType = typeof(Resources.RsBase))]
        Ssh,
        [Display(Name = "TELNET", ResourceType = typeof(Resources.RsBase))]
        Telnet
    }
}
