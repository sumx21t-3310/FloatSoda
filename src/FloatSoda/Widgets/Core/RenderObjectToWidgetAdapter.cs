using FloatSoda.Elements;
using FloatSoda.Render;

namespace FloatSoda.Widgets.Core;

public record RenderObjectToWidgetAdapter : RenderObjectWidget
{
    public RenderView Container { get; init; }
    public override Element CreateElement() => new RenderObjectToWidgetElement();

    public override RenderObject CreateRenderObject() => Container;

    public RenderObjectToWidgetElement AttachToRenderTree()
    {
        var element = CreateElement() as RenderObjectToWidgetElement;
        
        element!.Mount(null);
        
        return element;
    }
}

public class RenderObjectToWidgetElement : RenderObjectElement
{
    private Element? Child { get; set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Rebuild();
    }
    
    private void Rebuild()
    {
        
    }

    protected override void InsertRenderObjectChild(RenderObject child)
    {
        if (RenderObject is RenderView view)
        {
                
        }
    }
}