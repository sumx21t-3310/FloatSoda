using FloatSoda.Render;

namespace FloatSoda.Widgets.Core;

public abstract record RenderObjectWidget : Widget
{
    public abstract RenderObject CreateRenderObject();
}