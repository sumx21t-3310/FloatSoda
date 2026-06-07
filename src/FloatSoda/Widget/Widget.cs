using FloatSoda.Geometrics;
using R3;
using SkiaSharp;

namespace FloatSoda.Widget;

public abstract class Widget
{
    public IKey? Key { get; init; }
}

public abstract class StatelessWidget : Widget
{
    public abstract Widget Build(BuildContext context);
}


/// <summary>
/// ElementのRebuild Observable Stream Treeを購読するSubject/ReactiveProperty/ReactiveCollectionを返却します
/// </summary>
public static class HookExtension
{
    public static ReactiveProperty<T> UseState<T>(this BuildContext context, Func<T> initState) =>
        throw new NotImplementedException(); // React のuseState相当

    public static void UseEffect<T>(this BuildContext context, Func<IObserver<T>> onMount) =>
        throw new NotImplementedException();

    public static T Depends<T>(this BuildContext context, Func<IServiceProvider ,T> provider) =>
        throw new NotImplementedException(); // ServiceProviderから依存性を注入

    public static T UseMemo<T>(this BuildContext context, Func<T> func) => throw new NotImplementedException();
    public static Subject<T> UseAction<T>(this BuildContext context) => throw new NotImplementedException();
}

public class Center : StatelessWidget
{
    public Widget? Child { get; init; }

    public override Widget Build(BuildContext context) => throw new NotImplementedException();
}


public class SizedBox : StatelessWidget
{
    public double? Width { get; init; }
    public double? Height { get; init; }
    public Widget? Child { get; init; }

    public override Widget Build(BuildContext context) => throw new NotImplementedException();
}

public class Align : Widget
{
    public Widget? Child { get; init; }
    public Alignment Alignment { get; init; } = Alignment.Center;
}
public class ColoredBox : StatelessWidget
{
    public SKColor Color { get; init; } = SKColors.Black;
    public Widget? Child { get; init; }

    public override Widget Build(BuildContext context) => throw new NotImplementedException();
}


public record BuildContext;

public interface IKey;

public record struct ValueKey<T>(T Value) : IKey;

public record struct UniqueKey() : IKey
{
    public Guid Id { get; init; } = Guid.NewGuid();
}