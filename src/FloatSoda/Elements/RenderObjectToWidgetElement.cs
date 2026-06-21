using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class RenderObjectToWidgetElement<T> : RenderObjectElement<T> where T : RenderObject
{
    private Element? Child { get; set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Rebuild();
    }
    
    private void Rebuild() => Child = UpdateChild(Child, (Widget as RenderObjectToWidgetAdapter)?.Child);

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