namespace Core.Storage;

public abstract class DataService<T>(IDataStorage<T> dataStorage)
where T : IIdentifiable
{
    protected List<T> Items = [];

    public async Task LoadAsync()
    {
        Items = (await dataStorage.LoadAsync()).ToList();
    }

    public async Task AddAsync(T item)
    {
        Items.Add(item);
        await dataStorage.SaveAsync(Items);
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
        var item = GetOrExcept(id);

        Items.Remove(item);
        await dataStorage.SaveAsync(Items);
    }

    public async Task UpdateAsync(Guid id, Action<T> update)
    {
        var item = GetOrExcept(id);
        update(item);
        await dataStorage.SaveAsync(Items);
    }

    private T GetOrExcept(Guid id)
    {
        return GetById(id) ?? throw new KeyNotFoundException($"{typeof(T).Name} {id} doesn't exist.");
    }
}