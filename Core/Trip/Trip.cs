using Core.Storage;

namespace Core.Trip;

public enum TripStatus
{
    Scheduled,
    OnGoing,
    Completed
}

public class Trip(
    DateOnly departureDate,
    List<TripStop> stops,
    TripStatus status = TripStatus.Scheduled) : IIdentifiable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateOnly DepartureDate { get; set; } = departureDate;
    public TripStatus Status { get; private set; } = status;
    public List<TripStop> Stops { get; set; } = stops;
    public IEnumerable<Guid> OrderIds => Stops.Select(s => s.OrderId).Distinct();

    public void Start()
    {
        if (Status != TripStatus.Scheduled)
            throw new InvalidOperationException("Trip has already been started.");
        Status = TripStatus.OnGoing;
    }

    public void Complete()
    {
        if (Status != TripStatus.OnGoing)
            throw new InvalidOperationException("Cannot complete a trip that is not ongoing.");
        Status = TripStatus.Completed;
    }
}