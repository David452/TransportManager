namespace Core.Geocoding;

/*
 * Kódovenie query do GeoLokácie (lat, lon, display name)
 */
public interface IGeocodingService
{
    Task<GeoLocation?> GeocodeAsync(string query);
}