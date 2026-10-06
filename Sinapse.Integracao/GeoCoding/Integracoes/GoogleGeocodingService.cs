using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sinapse.Integracao.Geocoding.Google
{
    public class GoogleGeocodingService : IGeocodingService
    {
        private static int _failureCount = 0;
        private static DateTime? _circuitOpenUntil = null;

        private readonly string _apiKey;
        private readonly ILog _logger; // ex: log4net

        public GoogleGeocodingService(ILog logger, string apikey)
        {
            _apiKey = apikey;
            _logger = logger;
        }

        public async Task<GeocodingResult> GetCoordinatesAsync(string address, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
                return null;

            if (_circuitOpenUntil.HasValue && _circuitOpenUntil > DateTime.UtcNow)
            {
                _logger.Warn("Circuit breaker OPEN");
                return null;
            }

            var normalized = address.Trim().ToLowerInvariant();

            

            var url = $"maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var response = await new HttpClientProvider().Client.GetAsync(url, token);

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("Google API error");

                    var content = await response.Content.ReadAsStringAsync();

                    var json = JObject.Parse(content);

                    if ((string)json["status"] != "OK")
                        return null;

                    var location = json["results"][0]["geometry"]["location"];

                    var result = new GeocodingResult(
                        (double)location["lat"],
                        (double)location["lng"]);

                   

                    _failureCount = 0;

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error("Geocoding attempt failed", ex);

                    await Task.Delay(200 * attempt);

                    _failureCount++;

                    if (_failureCount >= 5)
                    {
                        _circuitOpenUntil = DateTime.UtcNow.AddSeconds(30);
                    }
                }
            }

            return null;
        }
    }

    public class HttpClientProvider
    {
        private readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://maps.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(5);

            return client;
        });

        public HttpClient Client => _client.Value;
    }
}