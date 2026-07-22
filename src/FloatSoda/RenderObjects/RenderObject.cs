using System.Diagnostics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public abstract class RenderObject : IHitTestTarget
{
    public BoxConstraints Constraints { get; private set; }
    public IParentData? ParentData { get; set; }

    public RenderObject? Parent { get; set; }

    public RenderPipeline? Owner { get; set; }

    public ILayer? Layer { get; set; }

    public abstract SKSize Size { get; protected set; }

    public bool NeedsPaint { get; set; } = true;

    public bool NeedsLayout { get; set; } = true;

    public int Depth { get; set; } = 0;

    public virtual bool IsRepaintBoundary { get; }

    public bool Attached => Owner != null;

    public RenderObject? RelayoutBoundary { get; set; }

    public virtual bool SizedByParent { get; } = false;

    public abstract void PerformLayout();

    public void Layout(BoxConstraints constraints, bool parentUseSize = false)
    {
        Debug.WriteLine("Call Layout");

        var relayoutBoundary = !parentUseSize || SizedByParent || constraints.IsTight || Parent == null
            ? this
            : Parent.RelayoutBoundary;

        if (!NeedsLayout && Constraints == constraints && RelayoutBoundary == relayoutBoundary) return;

        Constraints = constraints;

        if (RelayoutBoundary != null && relayoutBoundary != RelayoutBoundary)
        {
            VisitChildren(child => child.CleanChildRelayoutBoundary());
        }

        RelayoutBoundary = relayoutBoundary;

        PerformLayout();

        NeedsLayout = false;

        MarkNeedsPaint();
    }

    public void LayoutWithoutResize()
    {
        PerformLayout();

        NeedsLayout = false;

        MarkNeedsPaint();
    }

    public void CleanChildRelayoutBoundary()
    {
        if (RelayoutBoundary != this)
        {
            RelayoutBoundary = null;
            NeedsLayout = true;
            VisitChildren(child => child.CleanChildRelayoutBoundary());
        }
    }

    public void MarkNeedsLayout()
    {
        if (NeedsLayout) return;
        if (RelayoutBoundary != this)
        {
            MarkParentNeedsLayout();
        }
        else
        {
            NeedsLayout = true;
            Owner?.NodesNeedingLayout.Add(this);
            Owner?.RequestVisualUpdate();
        }
    }

    public void MarkParentNeedsLayout()
    {
        NeedsLayout = true;
        Parent?.MarkNeedsLayout();
    }


    public virtual void Attach(RenderPipeline? owner)
    {
        Debug.WriteLine("Call Attach");

        Owner = owner;

        if (NeedsLayout && RelayoutBoundary != null)
        {
            NeedsLayout = false;

            MarkNeedsLayout();
        }

        if (!NeedsPaint || Layer is null) return;

        NeedsPaint = false;
        MarkNeedsPaint();
    }

    public void AdoptChild(RenderObject child)
    {
        SetupParentData(child);

        MarkNeedsLayout();

        child.Parent = this;

        if (Attached)
        {
            child.Attach(Owner);
        }

        RedepthChild(child);
    }

    public void RedepthChild(RenderObject child)
    {
        if (child.Depth <= Depth)
        {
            child.Depth = Depth + 1;
            child.RedepthChildren();
        }
    }

    public virtual void RedepthChildren() { }

    public virtual void VisitChildren(Action<RenderObject> visitor) { }


    public virtual void SetupParentData(RenderObject child) { }


    public abstract void Paint(PaintingContext context, Offset offset);

    public void MarkNeedsPaint()
    {
        if (NeedsPaint) return;

        NeedsPaint = true;

        if (IsRepaintBoundary)
        {
            Owner?.NodesNeedingPaint.Add(this);
            Owner?.RequestVisualUpdate();
        }
        else
        {
            Parent?.MarkNeedsPaint();
        }
    }

    public virtual void Detach() => Owner = null;

    public void DropChild(RenderObject child)
    {
        child.CleanChildRelayoutBoundary();
        child.ParentData = null;
        child.Parent = null;

        if (Attached)
        {
            child.Detach();
        }

        MarkNeedsLayout();
    }

    
    public abstract void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry);
}
