using Core.Models;
using Core.Storage;

namespace Core.Services;


public class OrderService
{
    private readonly IDataStorage<Order> _dataStorage;
    private List<Order> _orders = new();

    public OrderService(IDataStorage<Order> dataStorage)
    {
        _dataStorage = dataStorage;
    }

    public async Task LoadAsync()
    {
        _orders = (await _dataStorage.LoadAsync()).ToList();
    }

    public async Task AddAsync(Order order)
    {
        _orders.Add(order);
        await _dataStorage.SaveAsync(_orders);
    }

    public List<Order> GetAll()
    {
        return _orders;
    }

    public Order? GetById(Guid id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public List<Order> GetByDestination(string destination)
    {
        return _orders.Where(o => o.Destination == destination).ToList();
    }

    public List<Order> GetByOrigin(string origin)
    {
        return _orders.Where(o => o.Origin == origin).ToList();
    }

    public List<Order> GetByStatus(OrderStatus status)
    {
        return _orders.Where(o => o.Status == status).ToList();
    }

    public async Task UpdateAsync(Order updated)
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