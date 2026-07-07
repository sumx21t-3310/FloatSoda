using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class RenderObjectToWidgetElement<T> : RenderObjectElement<T> where T : RenderObject
{
    public Widget? NewWidget { get; set; }
    private Element? Child { get; set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        OnRebuild();
    }

    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        OnRebuild();
    }

    private void OnRebuild() => Child = UpdateChild(Child, (Widget as RenderObjectToWidgetAdapter)?.Child);

    public override void PerformRebuild()
    {
        if (NewWidget is not null)
        {
            var tmp = NewWidget;
            NewWidget = null;
            Update(tmp);
        }

        base.PerformRebuild();
    }

    public override RenderObject? RenderObject { get; protected set; }

    public override void InsertRenderObjectChild(RenderObject? child)
    {
        if (RenderObject is IHasSingleChildRenderObject ro) ro.Child = child;
    }

    public override void VisitChildren(Action<Element> visitor)
    {
        if (Child != null) visitor(Child);
    }
}