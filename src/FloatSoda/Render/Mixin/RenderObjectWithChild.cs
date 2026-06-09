using FloatSoda.Common.Utils;

namespace FloatSoda.Render.Mixin;

public interface IRenderObjectWithChild<T> where T : RenderObject
{
    T? Child { get; set; }

    void SetRenderObject(T? child)
    {
        if (child == null || Child == child) return;
        Child = child;
        Child.ParentData = new BoxParentData();
    }

    void VisitChildren(Action<RenderObject> visitor) => Child.IfNotNull(visitor);
}