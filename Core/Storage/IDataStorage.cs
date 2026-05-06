namespace Core.Storage;

public interface IDataStorage<T>
{
    Task SaveAsync(IReadOnlyCollection<T> items);
    Task<IList<T>> LoadAsync();
}