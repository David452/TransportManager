namespace Core.Storage;

// Využitie generatívnej AI: ochrana in-memory kolekcie Items a perzistencie
// pomocou SemaphoreSlim, aby súbežné Add/Update/Delete operácie nepoškodili stav.
public abstract class DataService<T>(IDataStorage<T> dataStorage)
where T : IIdentifiable
{
    protected List<T> Items = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task LoadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            Items = (await dataStorage.LoadAsync()).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(T item)
    {
        await _lock.WaitAsync();
        try
        {
            Items.Add(item);
            await dataStorage.SaveAsync(Items);
        }
        finally
        {
            _lock.Release();
        }
    }

    public List<T> GetAll()
    {
        return Items;
    }

    public T? GetById(Guid id)
    {
        return Items.FirstOrDefault(item => item.Id == id);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var item = GetOrExcept(id);
            Items.Remove(item);
            await dataStorage.SaveAsync(Items);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(Guid id, Action<T> update)
    {
        await _lock.WaitAsync();
        try
        {
            var item = GetOrExcept(id);
            update(item);
            await dataStorage.SaveAsync(Items);
        }
        finally
        {
            _lock.Release();
        }
    }

    private T GetOrExcept(Guid id)
    {
        return GetById(id) ?? throw new KeyNotFoundException($"{typeof(T).Name} {id} doesn't exist.");
    }
}