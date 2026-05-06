using Core.Geocoding;
using Core.OSRM;

namespace Core.Trip;

public class TripBuilderService(IRouteService routeService)
{
    private List<Order.Order> _orders = [];

    public void AddOrder(Order.Order order)
    {
        _orders.Add(order);
    }

    public void RemoveOrder(Guid orderId)
    {
        _orders.RemoveAll(x => x.Id == orderId);
    }

    public async Task<IEnumerable<OrderSuggestion>> SuggestNearbyOrderAsync(IEnumerable<Order.Order> candidates, double thresholdKm)
    {
        var locations = new List<GeoLocation>();
        foreach (var order in _orders)
        {
            locations.Add(order.Origin);
            locations.Add(order.Destination);
        }

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
        return new Trip(departureDate, _orders);
    }
    
}