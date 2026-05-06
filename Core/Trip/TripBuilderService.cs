using Core.Geocoding;
using Core.Order;
using Core.OSRM;

namespace Core.Trip;

public class TripBuilderService(IRouteService routeService)
{
    private List<Order.Order> _orders = [];

    public void AddOrder(Order.Order order)
    {
        _orders.Add(order);
    }

    public void LoadFromTrip(IEnumerable<Order.Order> orders)
    {
        _orders = [..orders];
    }

    public void RemoveOrder(Guid orderId)
    {
        _orders.RemoveAll(x => x.Id == orderId);
    }

    public void SwapOrders(int firstIndex, int secondIndex)
    {
        (_orders[firstIndex], _orders[secondIndex]) = (_orders[secondIndex], _orders[firstIndex]);
    }

    public async Task<IEnumerable<OrderSuggestion>> SuggestNearbyOrderAsync(IEnumerable<Order.Order> candidates, double thresholdKm)
    {
        var locations = new List<GeoLocation>();
        foreach (var order in _orders)
        {
            locations.Add(order.Origin);
            locations.Add(order.Destination);
        }

        candidates = candidates.Where(o => o.Status == OrderStatus.New); // filtrovanie (mozu iba nove objednavky)
        
        var route = await routeService.GetRouteAsync(locations);
        var decodedPolyline = PolylineDecoder.Decode(route.EncodedPolyline);

        var suggestions = new List<OrderSuggestion>();
        foreach (var candidate in candidates)
        {
            var distanceKm = GeoMath.PointToRouteDistanceKm(candidate.Origin, decodedPolyline);
            if (distanceKm <= thresholdKm)
            {
                suggestions.Add(new OrderSuggestion(candidate, distanceKm));
            }
        }
        
        return suggestions.OrderBy(s => s.DistanceFromRouteKm);
    }

    public Trip Build(DateOnly departureDate)
    {
        return new Trip(departureDate, _orders.Select(o => o.Id).ToList());
    }
    
}