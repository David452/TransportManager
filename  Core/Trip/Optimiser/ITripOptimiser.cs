namespace Core.Trip.Optimiser;

public interface ITripOptimiser
{
    void Optimise(ref IEnumerable<Order.Order> orders);
}