using System.Text.Json;

namespace Core.Storage;

public class JsonDataStorage<T> : IDataStorage<T>
{

    private readonly string _path;
    public JsonDataStorage(string path)
    {
        _path = Path.Combine(AppContext.BaseDirectory, path);
    }

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };
    
    public async Task SaveAsync(IReadOnlyCollection<T> items)
    {
        var dir = Path.GetDirectoryName(_path);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        await using var stream = File.Create(_path);
        await JsonSerializer.SerializeAsync(stream, items, _options);
    }

    public async Task<IList<T>> LoadAsync()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        await using var stream = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<IList<T>>(stream, _options) ?? [];
    }
}