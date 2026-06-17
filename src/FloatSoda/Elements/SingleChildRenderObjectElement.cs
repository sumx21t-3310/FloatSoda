using FloatSoda.Render;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class SingleChildRenderObjectElement : RenderObjectElement
{
    public SingleChildRenderObjectWidget? WidgetCascade => Widget as SingleChildRenderObjectWidget;
    
    private Element? Child { get; set; }
    
    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Child = UpdateChild(Child, WidgetCascade?.Child);
    }

    protected override void InsertRenderObjectChild(RenderObject child)
    {
        if (RenderObject is IHasSingleChildRenderObject ro)
        {
            ro.SetChildObject(child);
        }
    }
}