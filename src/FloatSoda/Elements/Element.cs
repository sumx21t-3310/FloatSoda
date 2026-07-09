using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class Element : IBuildContext, IComparable<Element>
{
    public Widget? Widget { get; set; }

    public Element? Parent { get; set; }

    public int Depth { get; set; }

    public virtual RenderObject? RenderObject
    {
        get
        {
            RenderObject? result = null;

            Visit(this);

            return result;

            void Visit(Element element)
            {
                if (element is RenderObjectElement)
                {
                    result = element.RenderObject;
                }
                else
                {
                    element.VisitChildren(Visit);
                }
            }
        }
        protected set => field = value;
    }

    public Dictionary<Type, InheritedElement>? InheritedWidgets { get; set; }

    protected HashSet<InheritedElement> Dependencies { get; } = [];

    public BuildOwner? Owner { get; set; }

    private InheritedWidget DependOnInheritedElement(InheritedElement ancestor)
    {
        Dependencies.Add(ancestor);
        ancestor.UpdateDependencies(this);
        return ancestor.Widget as InheritedWidget;
    }

    public T? DependOnInheritedWidgetOfExactType<T>() where T : InheritedWidget
    {
        if (InheritedWidgets?.TryGetValue(typeof(T), out var ancestor) ?? false)
        {
            return DependOnInheritedElement(ancestor) as T;
        }

        return null;
    }

    public virtual InheritedElement? GetElementForInheritedWidgetOfExactType<T>() where T : InheritedWidget
    {
        if (InheritedWidgets?.TryGetValue(typeof(T), out var inheritedElement) ?? false)
        {
            return inheritedElement;
        }

        return null;
    }

    protected virtual void UpdateInheritance() => InheritedWidgets = Parent?.InheritedWidgets;

    public bool InDirtyList { get; set; }
    public bool Dirty { get; set; }

    public virtual void VisitChildren(Action<Element> visitor) { }

    public virtual void Mount(Element? parent)
    {
        Parent = parent;
        Depth = parent != null ? parent.Depth + 1 : 1;

        if (Parent != null)
        {
            Owner = Parent.Owner;
        }

        UpdateInheritance();
    }


    protected Element? UpdateChild(Element? child, Widget? newWidget)
    {
        if (newWidget == null)
        {
            if (child != null)
            {
                DeactivateChild(child);
            }

            return null;
        }


        if (child == null) return InflateWidget(newWidget);


        if (child.Widget == newWidget)
        {
            return child;
        }

        if (Widget.CanUpdate(child.Widget, newWidget))
        {
            child.Update(newWidget);
            return child;
        }

        DeactivateChild(child);
        return InflateWidget(newWidget);
    }

    public virtual void Update(Widget newWidget) => Widget = newWidget;


    protected Element InflateWidget(Widget newWidget)
    {
        var newChild = newWidget.CreateElement();

        newChild.Widget = newWidget;
        newChild.Mount(this);

        return newChild;
    }

    public virtual void AttachRenderObject() { }

    public virtual void DetachRenderObject()
    {
        VisitChildren(child => child.DetachRenderObject());
    }

    protected void DeactivateChild(Element child)
    {
        child.Parent = null;
        child.DetachRenderObject();
    }

    public virtual void DidChangeDependencies() => MarkNeedsBuild();

    public void MarkNeedsBuild()
    {
        if (Dirty) return;

        Dirty = true;
        Owner?.ScheduledBuildFor(this);
    }

    /// <summary>
    /// ホットリロード時に呼ばれ、このElement以下のサブツリー全体を再ビルド対象にする。
    /// Widgetのrecord等価による差分スキップに関わらず、全ComponentElementのBuild()が再実行される。
    /// </summary>
    public void Reassemble()
    {
        MarkNeedsBuild();
        VisitChildren(child => child.Reassemble());
    }

    public void Rebuild() => PerformRebuild();

    public abstract void PerformRebuild();

    public int CompareTo(Element? other)
    {
        if (Depth < other.Depth) return -1;
        if (Depth > other.Depth) return 1;
        if (!Dirty && other.Dirty) return -1;
        if (Dirty && !other.Dirty) return 1;
        return 0;
    }
}