using Core.Storage;

namespace Core.Geocoding;

public class CombinedGeocodingService : IGeocodingService
{
    private readonly LocalGeocodingService _local;
    private readonly IGeocodingService _external;

    public CombinedGeocodingService(LocalGeocodingService local, IGeocodingService external)
    {
        _external = external;
        _local = local;
    }

    public async Task<GeoLocation?> GeocodeAsync(string query)
    {
        // 1. pokus o najdenie lokalne
        var result = await _local.GeocodeAsync(query);
        if (result is not null)
        {
            return result;
        }
        
        // 2. Hladanie pomocou externeho api
        result = await _external.GeocodeAsync(query);
        if (result is not null)
        {
            // Ukladanie do lokalnej db
            await _local.AddLocationAsync(result);
        }
        
        return result;
    }
}