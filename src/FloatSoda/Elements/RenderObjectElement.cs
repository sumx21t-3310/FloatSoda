using FloatSoda.Render;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class RenderObjectElement : Element
{
    public override RenderObject? RenderObject { get; protected set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);

        if (Widget is RenderObjectWidget renderObjectWidget)
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

    private RenderObjectElement? FindAncestorRenderObjectElement()
    {
        var ancestor = Parent;

        while (ancestor != null && ancestor is not RenderObjectElement)
        {
            ancestor = ancestor.Parent;
        }

        return ancestor as RenderObjectElement;
    }

    protected virtual void InsertRenderObjectChild(RenderObject child)
    {
    }
}