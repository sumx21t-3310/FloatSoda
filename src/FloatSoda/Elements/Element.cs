using System.Diagnostics;
using FloatSoda.Render;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class Element : IBuildContext
{
    public virtual Widget? Widget { get; set; }

    public Element? Parent { get; set; }

    public virtual RenderObject? RenderObject
    {
        get
        {
            RenderObject? result = null;
            VisitChildren(child => result = child.RenderObject);
            return result;
        }
        protected set;
    } = null;

    public virtual void VisitChildren(Action<Element> visitor)
    {
    }

    public virtual void Mount(Element? parent) => Parent = parent;

    protected Element? UpdateChild(Element? child, Widget? newWidget)
    {
        Debug.Assert(child == null);
        return newWidget != null ? InflateWidget(newWidget) : null;
    }

    protected Element InflateWidget(Widget newWidget)
    {
        var newChild = newWidget.CreateElement();
        newChild.Mount(this);
        return newChild;
    }

    protected virtual void AttachRenderObject()
    {
    }
}