using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public record ServiceScope : InheritedWidget
{
    public required IServiceProvider Services { get; init; }

    public static IServiceProvider Of(IBuildContext context)
    {
        return context.DependOnInheritedWidgetOfExactType<ServiceScope>()?.Services ??
               throw new InvalidOperationException("ServiceScopeが祖先に見つかりません。");
    }

    public override bool UpdateShouldNotify(InheritedWidget oldWidget)
    {
        return oldWidget is ServiceScope oldScope && !ReferenceEquals(oldScope.Services, Services);
    }
}

public static class ServiceScopeExtension
{
    public static T Resolve<T>(this IBuildContext context) where T : notnull
    {
        return (T)ServiceScope.Of(context).GetService(typeof(T))!;
    }
}