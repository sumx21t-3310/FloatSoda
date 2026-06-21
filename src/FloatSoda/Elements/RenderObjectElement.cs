using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class RenderObjectElement<T> : Element where T : RenderObject
{
    public override RenderObject? RenderObject { get; protected set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);

        if (Widget is RenderObjectWidget<T> renderObjectWidget)
        {
            RenderObject = renderObjectWidget.CreateRenderObject();
        }

        AttachRenderObject();
    }

    protected override void AttachRenderObject()
    {
        var ancestorRenderObjectElement = FindAncestorRenderObjectElement();
        ancestorRenderObjectElement?.InsertRenderObjectChild(RenderObject);
    }

    private RenderObjectElement<T>? FindAncestorRenderObjectElement()
    {
        var ancestor = Parent;

        while (ancestor != null && ancestor is not RenderObjectElement<T>)
        {
            ancestor = ancestor.Parent;
        }

        return ancestor as RenderObjectElement<T>;
    }

    protected virtual void InsertRenderObjectChild(RenderObject child)
    {
    }
}