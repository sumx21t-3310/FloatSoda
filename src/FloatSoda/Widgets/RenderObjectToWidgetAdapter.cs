using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

public record RenderObjectToWidgetAdapter : RenderObjectWidget<RenderView>
{
    public required RenderView Container { get; init; }
    public Widget? Child { get; init; }
    public override Element CreateElement() => new RenderObjectToWidgetElement<RenderView>();

    public override RenderView CreateRenderObject() => Container;

    public RenderObjectToWidgetElement<RenderView> AttachToRenderTree(BuildOwner owner, RenderObjectToWidgetElement<RenderView>? element)
    {
        RenderObjectToWidgetElement<RenderView> result;
        if (element == null)
        {
            result = (RenderObjectToWidgetElement<RenderView>)CreateElement();
            result.Widget = this;
            result.Owner = owner;
            owner.BuildScope(() => result.Mount(null));
        }
        else
        {
            result = element;
            result.NewWidget = this;
            result.MarkNeedsBuild();
        }

        return result;
    }
}