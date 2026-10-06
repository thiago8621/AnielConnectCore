using Sinapse.LGPD.Models;

namespace Sinapse.LGPD.Interfaces
{
    public interface IWatermarkService
    {
        byte[] ApplySecurityWatermark(byte[] imageData, WatermarkRequest request);
    }
}