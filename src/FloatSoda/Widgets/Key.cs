namespace FloatSoda.Widgets;

public interface IKey;

public record struct ValueKey<T>(T Value) : IKey;

public record struct UniqueKey() : IKey
{
    public Guid Id { get; init; } = Guid.NewGuid();
}