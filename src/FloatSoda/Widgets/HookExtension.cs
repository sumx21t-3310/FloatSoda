using FloatSoda.Elements;
using R3;

namespace FloatSoda.Widgets;

/// <summary>
/// ElementのRebuild Observable Stream Treeを購読するSubject/ReactiveProperty/ReactiveCollectionを返却します
/// </summary>
public static class HookExtension
{
    public static ReactiveProperty<T> UseState<T>(this IBuildContext context, Func<T> initState) =>
        throw new NotImplementedException(); // React のuseState相当

    public static void UseEffect<T>(this IBuildContext context, Func<IObserver<T>> onMount) =>
        throw new NotImplementedException();

    public static T Depends<T>(this IBuildContext context, Func<IServiceProvider ,T> provider) =>
        throw new NotImplementedException(); // ServiceProviderから依存性を注入

    public static T UseMemo<T>(this IBuildContext context, Func<T> func) => throw new NotImplementedException();
    public static Subject<T> UseAction<T>(this IBuildContext context) => throw new NotImplementedException();
}