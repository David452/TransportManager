using System.Text.Json;

namespace Core.Storage;

// Využitie generatívnej AI: synchronizácia súbežných čítaní/zápisov do JSON
// súboru pomocou SemaphoreSlim (prevencia race condition pri Save/Load).
public class JsonDataStorage<T> : IDataStorage<T>
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock = new(1, 1);

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

        await _lock.WaitAsync();
        try
        {
            await using var stream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, items, _options);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IList<T>> LoadAsync()
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        await _lock.WaitAsync();
        try
        {
            await using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await JsonSerializer.DeserializeAsync<IList<T>>(stream, _options) ?? [];
        }
        finally
        {
            _lock.Release();
        }
    }
}
