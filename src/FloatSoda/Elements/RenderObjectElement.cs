using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class RenderObjectElement : Element
{
    public abstract override RenderObject? RenderObject { get; protected set; }

    protected override void AttachRenderObject()
    {
        FindAncestorRenderObjectElement()?.InsertRenderObjectChild(RenderObject);
    }

    private RenderObjectElement? FindAncestorRenderObjectElement()
    {
        var ancestor = Parent;
        while (ancestor != null && ancestor is not RenderObjectElement)
            ancestor = ancestor.Parent;
        return ancestor as RenderObjectElement;
    }

    protected virtual void InsertRenderObjectChild(RenderObject child) { }
}

public abstract class RenderObjectElement<T> : RenderObjectElement where T : RenderObject
{
    public override RenderObject? RenderObject { get; protected set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);

        if (Widget is RenderObjectWidget<T> renderObjectWidget)
            RenderObject = renderObjectWidget.CreateRenderObject();

        AttachRenderObject();
    }
}
