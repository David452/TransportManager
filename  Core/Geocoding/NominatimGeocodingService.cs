using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Geocoding;

/**
 * Zadarmo API je obmedzene na 1 req/s
 * Pretvára query (string) na GeoLocation pomocou externeho API
 */
public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://nominatim.openstreetmap.org/search";

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTime _lastRequest = DateTime.MinValue;

    public NominatimGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TransportManager/1.0");
    }

    public async Task<GeoLocation?> GeocodeAsync(string query)
    {
        await _semaphore.WaitAsync();
        try
        {
            var elapsed = DateTime.UtcNow - _lastRequest;
            if (elapsed < TimeSpan.FromSeconds(1))
                await Task.Delay(TimeSpan.FromSeconds(1) - elapsed);

            _lastRequest = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }

        var encoded = Uri.EscapeDataString(query);
        var url = $"{BaseUrl}?q={encoded}&format=json&limit=1";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<List<NominatimResult>>(json) ?? [];

        return doc.Select(r => new GeoLocation
        {
            Latitude = double.Parse(r.Lat, CultureInfo.InvariantCulture),
            Longitude = double.Parse(r.Lon, CultureInfo.InvariantCulture),
            DisplayName = r.DisplayName,
        }).FirstOrDefault();
    }
}

internal class NominatimResult(string lat, string lon)
{
    [JsonPropertyName("lat")]
    public required string Lat  { get; init; } = lat;

    [JsonPropertyName("lon")]
    public required string Lon { get; init; } = lon;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;
}