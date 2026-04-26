using Core.Trip.Optimiser;

namespace Core.Trip;

public class TripBuilderService(ITripOptimiser tripOptimiser)
{
    private ITripOptimiser _tripOptimiser = tripOptimiser;
    private IEnumerable<Order.Order> _orders = [];

    public void AddOrder(Order.Order order)
    {
        throw new NotImplementedException();
    }

    public void RemoveOrder(Guid orderId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<OrderSuggestion>> SuggestNearbyOrderAsync(IEnumerable<Order.Order> candidates, double thresholdKm)
    {
        throw new NotImplementedException();
    }

    public async Task<Trip> BuildAsync(DateOnly departureDate)
    {
        throw new NotImplementedException();
    }
    
    public async Task OptimizeAsync()
    {
        throw new NotImplementedException();
    }
}