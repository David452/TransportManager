using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Geocoding;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://nominatim.openstreetmap.org/search";

    public NominatimGeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TransportManager/1.0");
    }


    public async Task<GeoLocation?> GeocodeAsync(string query)
    {
        var encoded = Uri.EscapeDataString(query);
        var url = $"{BaseUrl}?q={encoded}&format=json&limit=1";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonSerializer.Deserialize<List<NominatimResult>>(json) ?? new();

        return doc.Select(r => new GeoLocation
        {
            Latitude = double.Parse(r.Lat, CultureInfo.InvariantCulture),
            Longitude = double.Parse(r.Lon, CultureInfo.InvariantCulture),
            DisplayName = r.DisplayName,
        }).FirstOrDefault();
    }
}

internal class NominatimResult
{
    [JsonPropertyName("lat")]
    public string Lat  { get; set; }
    [JsonPropertyName("lon")]
    public string Lon { get; set; }
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = String.Empty;
}