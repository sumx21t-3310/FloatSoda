using FloatSoda.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace FloatSoda.Widgets;

public record ServiceProvider : InheritedWidget
{
    public required IServiceProvider Services { get; init; }

    public static IServiceProvider Of(IBuildContext context)
    {
        return context.DependOnInheritedWidgetOfExactType<ServiceProvider>()?.Services ??
               throw new InvalidOperationException("ServiceScopeが祖先に見つかりません。");
    }

    public override bool UpdateShouldNotify(InheritedWidget oldWidget)
    {
        return oldWidget is ServiceProvider oldScope && !ReferenceEquals(oldScope.Services, Services);
    }
}

public static class ServiceScopeExtension
{
    public static T GetRequiredService<T>(this IBuildContext context) where T : notnull
    {
        return ServiceProvider.Of(context).GetRequiredService<T>();
    }

    public static T? GetService<T>(this IBuildContext context)
    {
        return ServiceProvider.Of(context).GetService<T>();
    }
}