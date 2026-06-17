using FloatSoda.Render;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class RenderObjectToWidgetElement : RenderObjectElement
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
        if (RenderObject is IHasSingleChildRenderObject ro)
        {
            ro.SetChildObject(child);
        }
    }
}