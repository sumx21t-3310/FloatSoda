using FloatSoda.Elements;
using FloatSoda.Render;

namespace FloatSoda.Widgets;

public record RenderObjectToWidgetAdapter : RenderObjectWidget
{
    public required RenderView Container { get; init; }
    public Widget? Child { get; init; }
    public override Element CreateElement() => new RenderObjectToWidgetElement();

    public override RenderObject CreateRenderObject() => Container;

    public RenderObjectToWidgetElement AttachToRenderTree()
    {
        var element = CreateElement() as RenderObjectToWidgetElement;

        element!.Widget = this;
        element!.Mount(null);

        return element;
    }
}