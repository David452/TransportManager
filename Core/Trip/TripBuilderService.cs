using Core.Geocoding;
using Core.Order;
using Core.OSRM;

namespace Core.Trip;

public class TripBuilderService(IRouteService routeService)
{
    private List<TripStop> _stops = [];
    private readonly Dictionary<Guid, Order.Order> _orders = new();

    public void AddOrder(Order.Order order)
    {
        _orders[order.Id] = order;
        _stops.Add(new TripStop(order.Id, StopType.Pickup));
        _stops.Add(new TripStop(order.Id, StopType.Dropoff));
    }

    public void LoadFromTrip(IEnumerable<Order.Order> orders)
    {
        foreach (var order in orders)
        {
            AddOrder(order);
        }
        
    }

    public void RemoveOrder(Guid orderId)
    {
        _stops.RemoveAll(x => x.OrderId == orderId);
    }

    public void SwapStops(int firstIndex, int secondIndex)
    {
        (_stops[firstIndex], _stops[secondIndex]) = (_stops[secondIndex], _stops[firstIndex]);
    }

    public async Task<IEnumerable<OrderSuggestion>> SuggestNearbyOrderAsync(IEnumerable<Order.Order> candidates, double thresholdKm)
    {
        var locations = new List<GeoLocation>();
        foreach (var stop in _stops)
        {
            var order = _orders[stop.OrderId];
            locations.Add(stop.Type == StopType.Pickup? order.Origin : order.Destination);
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
        return new Trip(departureDate, _stops);
    }
    
}