using System.Diagnostics;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class Element : IBuildContext
{
    public Widget? Widget { get; set; }

    public Element? Parent { get; set; }

    public virtual RenderObject? RenderObject
    {
        get
        {
            RenderObject? result = null;

            void Visit(Element element)
            {
                if (element is RenderObjectElement renderObjectElement)
                {
                    result = renderObjectElement.RenderObject;
                }
                else
                {
                    element.VisitChildren(Visit);
                }
            }

            Visit(this);

            return result;
        }
        protected set => field = value;
    }

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
        newChild.Widget = newWidget;
        newChild.Mount(this);
        return newChild;
    }

    protected virtual void AttachRenderObject()
    {
    }

    public virtual void PerformRebuild()
    {
    }
}