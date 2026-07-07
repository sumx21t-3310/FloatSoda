using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class MultiChildRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    public MultiChildRenderObjectWidget<T> WidgetCasted => (MultiChildRenderObjectWidget<T>)Widget!;

    private List<Element> Children { get; set; } = [];

    public override RenderObject? RenderObject { get; protected set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Children = WidgetCasted.Children.Select(InflateWidget).ToList();
    }

    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Children = UpdateChildren(Children, WidgetCasted.Children);
    }

    public override void VisitChildren(Action<Element> visitor) => Children.ForEach(visitor);

    public override void InsertRenderObjectChild(RenderObject child)
    {
        if (RenderObject is IHasMultiChildrenRenderObject ro) ro.AddChild(child);
    }

    public override void RemoveRenderObjectChild(RenderObject? child)
    {
        if (child != null && RenderObject is IHasMultiChildrenRenderObject ro) ro.RemoveChild(child);
    }
}