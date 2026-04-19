namespace Core.Geocoding;

public interface IGeocodingService
{
    Task<GeoLocation?> GeocodeAsync(string query);
}