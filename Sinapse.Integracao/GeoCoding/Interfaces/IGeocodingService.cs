using System.Threading;
using System.Threading.Tasks;

namespace Sinapse.Integracao.Geocoding.Google
{
    public sealed class GeocodingResult
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeocodingResult(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }
    public interface IGeocodingService
    {
        Task<GeocodingResult> GetCoordinatesAsync(string address, CancellationToken token);
    }

}