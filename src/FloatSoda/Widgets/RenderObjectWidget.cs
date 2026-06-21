using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

public abstract record RenderObjectWidget : Widget
{
    public abstract RenderObject CreateRenderObject();
}