using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class SingleChildRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    public SingleChildRenderObjectWidget<T>? WidgetCasted => Widget as SingleChildRenderObjectWidget<T>;

    private Element? Child { get; set; }

    public override RenderObject? RenderObject { get; protected set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Child = UpdateChild(Child, WidgetCasted?.Child);
    }


    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Child = UpdateChild(Child, WidgetCasted?.Child);
    }

    public override void InsertRenderObjectChild(RenderObject? child)
    {
        if (RenderObject is IHasSingleChildRenderObject ro) ro.Child = child;
    }

    public override void RemoveRenderObjectChild(RenderObject? child)
    {
        if (RenderObject is IHasSingleChildRenderObject ro) ro.Child = null;
    }

    public override void VisitChildren(Action<Element> visitor)
    {
        if (Child != null) visitor(Child);
    }
}