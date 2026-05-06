namespace Core.Storage;

public interface IIdentifiable
{
    Guid Id { get; init; }
}