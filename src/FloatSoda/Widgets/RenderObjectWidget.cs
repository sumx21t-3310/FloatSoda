using FloatSoda.Render;

namespace FloatSoda.Widgets;

public abstract record RenderObjectWidget : Widget
{
    public abstract RenderObject CreateRenderObject();
}