using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

public record RenderObjectToWidgetAdapter : RenderObjectWidget<RenderView>
{
    public required RenderView Container { get; init; }
    public Widget? Child { get; init; }
    public override Element CreateElement() => new RenderObjectToWidgetElement<RenderView>();

    public override RenderView CreateRenderObject() => Container;

    public RenderObjectToWidgetElement<RenderView> AttachToRenderTree()
    {
        var element = CreateElement() as RenderObjectToWidgetElement<RenderView>;

        element!.Widget = this;
        element!.Mount(null);

        return element;
    }
}