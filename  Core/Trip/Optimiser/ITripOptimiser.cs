namespace Core.Trip.Optimiser;

public interface ITripOptimiser
{
    void Optimise(ref List<Order.Order> orders);
}