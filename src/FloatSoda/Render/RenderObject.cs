using System.Diagnostics;
using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public abstract class RenderObject
{
    public BoxConstraints Constraints { get; private set; }
    public IParentData? ParentData { get; set; }

    public RenderObject? Parent { get; set; }

    public bool NeedsPaint { get; set; } = true;
    
    public virtual bool IsRepaintBoundary { get; }
    
    public RenderPipeline? Owner { get; set; }
    
    public ILayer? Layer { get; set; }
    
    public bool Attached => Owner != null;

    public abstract SKSize Size { get; protected set; }

    public abstract void PerformLayout();

    public void Layout(BoxConstraints constraints)
    {
        Debug.WriteLine("Call Layout");
        Constraints = constraints;
        PerformLayout();
        MarkNeedsPaint();
    }

    public virtual void Attach(RenderPipeline? owner)
    {
        Debug.WriteLine("Call Attach");

        Owner = owner;

        if (!NeedsPaint || Layer is null) return;
        
        NeedsPaint = false;
        MarkNeedsPaint();
    }
    public virtual void SetupParentData(RenderObject child) {}

    public void AdoptChild(RenderObject child)
    {
        SetupParentData(child);
        child.Parent = this;
        if (Attached)
        {
            child.Attach(Owner);
        }
    }
    
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
}