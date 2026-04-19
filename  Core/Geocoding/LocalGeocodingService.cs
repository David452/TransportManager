using Core.Storage;

namespace Core.Geocoding;

public class LocalGeocodingService : IGeocodingService
{
    private List<GeoLocation> _locations = new(); 
    private readonly IDataStorage<GeoLocation> _dataStorage;

    public LocalGeocodingService(IDataStorage<GeoLocation> dataStorage)
    {
        _dataStorage = dataStorage;
    }

    public async Task LoadAsync()
    {
        _locations = (await _dataStorage.LoadAsync()).ToList();
    }

    public async Task AddLocationAsync(GeoLocation location)
    {
        _locations.Add(location);
        await _dataStorage.SaveAsync(_locations);
    }

    public Task<GeoLocation?> GeocodeAsync(string query)
    {
        var result = _locations.FirstOrDefault(l =>
            l.DisplayName.Equals(query, StringComparison.OrdinalIgnoreCase));
        if (result is not null)
        {
            return Task.FromResult<GeoLocation?>(result);
        }
        
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        result = _locations.FirstOrDefault(l =>
            words.All(word =>
                l.DisplayName.Contains(word, StringComparison.OrdinalIgnoreCase)));
        
        return Task.FromResult(result);
    }
}