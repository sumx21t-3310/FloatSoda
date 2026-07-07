using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

public abstract record RenderObjectWidget<T> : Widget where T : RenderObject
{
    public abstract T CreateRenderObject();

    public virtual void UpdateRenderObject(T renderObject) {}
}