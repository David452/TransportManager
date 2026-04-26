using Core.Geocoding;
using Core.Storage;

namespace Core.Order;


public class OrderService
{
    private readonly IDataStorage<Core.Order.Order> _dataStorage;
    private List<Core.Order.Order> _orders = new();

    public OrderService(IDataStorage<Core.Order.Order> dataStorage)
    {
        _dataStorage = dataStorage;
    }

    public async Task LoadAsync()
    {
        _orders = (await _dataStorage.LoadAsync()).ToList();
    }

    public async Task AddAsync(Core.Order.Order order)
    {
        _orders.Add(order);
        await _dataStorage.SaveAsync(_orders);
    }

    public List<Core.Order.Order> GetAll()
    {
        return _orders;
    }

    public Core.Order.Order? GetById(Guid id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public List<Core.Order.Order> GetByDestination(GeoLocation destination)
    {
        return _orders.Where(o => o.Destination.DisplayName == destination.DisplayName).ToList();
    }

    public List<Core.Order.Order> GetByOrigin(GeoLocation origin)
    {
        return _orders.Where(o => o.Origin.DisplayName == origin.DisplayName).ToList();
    }

    public List<Core.Order.Order> GetByStatus(OrderStatus status)
    {
        return _orders.Where(o => o.Status == status).ToList();
    }

    public async Task UpdateAsync(Core.Order.Order updated)
    {
        var existing = GetById(updated.Id) 
                       ?? throw new KeyNotFoundException($"Order {updated.Id} doesn't exist.");
        
        existing.Origin = updated.Origin;
        existing.Destination = updated.Destination;
        existing.Note = updated.Note;   
        
        await _dataStorage.SaveAsync(_orders);
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = GetById(id)
                    ?? throw new KeyNotFoundException($"Order {id.ToString()} doesn't exist.");

        _orders.Remove(order);
        await _dataStorage.SaveAsync(_orders);
    }
}