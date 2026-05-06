using Core.Storage;

namespace Core.Trip;

public class TripService(IDataStorage<Trip> dataStorage)
    : DataService<Trip>(dataStorage)
{
    public List<Trip> GetByStatus(TripStatus status)
    {
        return Items.Where(t => t.Status == status).ToList();
    }
}