using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class SingleChildRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    public SingleChildRenderObjectWidget<T>? WidgetCascade => Widget as SingleChildRenderObjectWidget<T>;

    private Element? Child { get; set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Child = UpdateChild(Child, WidgetCascade?.Child);
    }

    protected override void InsertRenderObjectChild(RenderObject child)
    {
        if (RenderObject is IHasSingleChildRenderObjectBase ro)
            ro.SetChildObject(child);
    }

    public override void VisitChildren(Action<Element> visitor)
    {
        if (Child != null) visitor(Child);
    }
}