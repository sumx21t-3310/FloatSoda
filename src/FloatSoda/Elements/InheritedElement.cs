using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class InheritedElement : ComponentElement
{
    internal HashSet<Element> Dependents { get; } = [];
    public override Widget Build() => (Widget as InheritedWidget)!.Child;

    protected override void UpdateInheritance()
    {
        var incomingWidgets = Parent?.InheritedWidgets;

        InheritedWidgets = incomingWidgets != null
            ? new Dictionary<Type, InheritedElement>(incomingWidgets)
            : [];

        // 祖先マップの有無にかかわらず自分自身を登録する（同型はネストした側で上書き＝最近傍が優先）
        InheritedWidgets[Widget!.GetType()] = this;
    }

    public void UpdateDependencies(Element dependent) => Dependents.Add(dependent);

    public void RemoveDependent(Element dependent) => Dependents.Remove(dependent);

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