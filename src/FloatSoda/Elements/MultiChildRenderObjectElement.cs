using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class MultiChildRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    public MultiChildRenderObjectWidget<T> WidgetCascade => (MultiChildRenderObjectWidget<T>)Widget!;

    private List<Element> Children { get; set; } = [];

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Children = (WidgetCascade.Children ?? []).Select(InflateWidget).ToList();
    }

    protected override void InsertRenderObjectChild(RenderObject child)
    {
        if (RenderObject is IHasMultiChildrenRenderObjectBase ro)
            ro.Insert(child);
    }

    public override void VisitChildren(Action<Element> visitor)
        => Children.ForEach(visitor);
}