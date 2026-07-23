using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// InheritedWidgetを祖先マップへ登録し、その値に依存する子孫Elementへ変更を通知するElementです。
/// </summary>
/// <seealso cref="InheritedWidget"/>
public class InheritedElement : ComponentElement
{
    internal HashSet<Element> Dependents { get; } = [];
    /// <summary>
    /// 管理しているInheritedWidgetの子Widgetを返します。
    /// </summary>
    /// <returns>InheritedWidgetが公開する子Widget。</returns>
    public override Widget Build() => (Widget as InheritedWidget)!.Child;

    /// <summary>
    /// 親から継承した祖先マップを複製し、このInheritedWidgetを登録型の最近傍要素として追加します。
    /// </summary>
    protected override void UpdateInheritance()
    {
        var incomingWidgets = Parent?.InheritedWidgets;

        InheritedWidgets = incomingWidgets != null
            ? new Dictionary<Type, InheritedElement>(incomingWidgets)
            : [];

        // 祖先マップの有無にかかわらず自分自身を登録する（同型はネストした側で上書き＝最近傍が優先）
        InheritedWidgets[((InheritedWidget)Widget!).ScopeType] = this;
    }

    /// <summary>
    /// 指定したElementを、このInheritedWidgetの変更通知先として登録します。
    /// </summary>
    /// <param name="dependent">このInheritedWidgetの値に依存するElement。</param>
    public void UpdateDependencies(Element dependent) => Dependents.Add(dependent);

    /// <summary>
    /// 指定したElementを、このInheritedWidgetの変更通知先から除外します。
    /// </summary>
    /// <param name="dependent">依存関係を解除するElement。</param>
    public void RemoveDependent(Element dependent) => Dependents.Remove(dependent);

    /// <summary>
    /// 管理するInheritedWidgetを置き換え、通知条件を満たす場合は登録済みの依存Elementへ変更を通知します。
    /// </summary>
    /// <param name="newWidget">同じスコープを引き継ぐ新しいInheritedWidget。</param>
    /// <remarks>
    /// 更新後はこのElementを再構築し、依存先へ通知した場合は各依存Elementにも再構築を要求します。
    /// </remarks>
    public override void Update(Widget newWidget)
    {
        var oldWidget = Widget as InheritedWidget;
        base.Update(newWidget);

        if ((Widget as InheritedWidget).UpdateShouldNotify(oldWidget))
        {
            NotifyClients();
        }

        Dirty = true;
        Rebuild();
    }

    private void NotifyClients()
    {
        foreach (var dependent in Dependents)
        {
            dependent.DidChangeDependencies();
        }
    }
}