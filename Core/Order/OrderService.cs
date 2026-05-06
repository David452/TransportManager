using Core.Geocoding;
using Core.Storage;

namespace Core.Order;


public class OrderService(IDataStorage<Order> dataStorage) : DataService<Order>(dataStorage)
{

    public List<Order> GetByDestination(GeoLocation destination)
    {
        return Items.Where(o => o.Destination.DisplayName == destination.DisplayName).ToList();
    }

    public List<Order> GetByOrigin(GeoLocation origin)
    {
        return Items.Where(o => o.Origin.DisplayName == origin.DisplayName).ToList();
    }

    public List<Order> GetByStatus(OrderStatus status)
    {
        return Items.Where(o => o.Status == status).ToList();
    }
}