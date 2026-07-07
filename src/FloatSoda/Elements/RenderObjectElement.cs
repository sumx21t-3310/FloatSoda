using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class RenderObjectElement : Element
{
    private RenderObjectElement? _ancestorRenderObjectElement;


    public override void AttachRenderObject()
    {
        _ancestorRenderObjectElement = FindAncestorRenderObjectElement();
        _ancestorRenderObjectElement?.InsertRenderObjectChild(RenderObject);
    }


    public override void DetachRenderObject()
    {
        if (_ancestorRenderObjectElement == null) return;
        _ancestorRenderObjectElement.RemoveRenderObjectChild(RenderObject);
        _ancestorRenderObjectElement = null;
    }


    public virtual void InsertRenderObjectChild(RenderObject? child)
    {
    }


    public virtual void RemoveRenderObjectChild(RenderObject? child)
    {
    }


    public RenderObjectElement? FindAncestorRenderObjectElement()
    {
        var ancestor = Parent;

        while (ancestor != null && ancestor is not RenderObjectElement)
        {
            ancestor = ancestor.Parent;
        }

        return ancestor as RenderObjectElement;
    }
}

public abstract class RenderObjectElement<T> : RenderObjectElement where T : RenderObject
{
    public abstract override RenderObject? RenderObject { get; protected set; }
    private RenderObjectWidget<T>? WidgetCascaded => Widget as RenderObjectWidget<T>;


    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        if (Widget is RenderObjectWidget<T> renderObjectWidget) RenderObject = renderObjectWidget.CreateRenderObject();
        AttachRenderObject();
        Dirty = false;
    }


    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        PerformRebuild();
    }


    public override void PerformRebuild()
    {
        WidgetCascaded?.UpdateRenderObject(RenderObject as T);
        Dirty = false;
    }
}
