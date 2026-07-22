using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public class RenderView : RenderObject, IHasSingleChildRenderObject
{
    public override SKSize Size { get; protected set; }

    private readonly SingleChildContainer<RenderBox> _child;

    public RenderBox? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    RenderObject? IHasSingleChildRenderObject.Child
    {
        get => Child;
        set => Child = (RenderBox?)value;
    }

    public RenderView(float width = 1000, float height = 1000)
    {
        Size = new SKSize(width, height);
        _child = new SingleChildContainer<RenderBox>(this);
    }

    public override bool IsRepaintBoundary => true;

    /// <summary>
    /// ウィンドウの固定サイズ。null の場合は子のレイアウト結果サイズに追従します。
    /// </summary>
    public SKSize? FixedSize
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>
    /// <see cref="FixedSize"/> があれば Tight 制約でレイアウトし、なければ Loosen（無限上限）制約を
    /// 渡して子ウィジェットのレイアウト結果サイズを <see cref="Size"/>（＝オーバーレイサイズ）に採用します。
    /// </summary>
    public override void PerformLayout()
    {
        if (Child == null)
        {
            Size = FixedSize ?? SKSize.Empty;
            return;
        }

        var constraints = FixedSize is { } fixedSize
            ? BoxConstraints.Tight(fixedSize)
            : BoxConstraints.Unbounded;

        Child.Layout(constraints, parentUseSize: true);
        Size = FixedSize ?? Child.Size;
    }


    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null) context.PaintChild(Child, offset);
    }

    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    public override void RedepthChildren() => VisitChildren(RedepthChild);

    public void PrepareInitialFrame()
    {
        ScheduleInitialLayout();
        ScheduleInitialPaint(new TransformLayer());
    }

    private void ScheduleInitialPaint(ContainerLayer rootLayer)
    {
        Layer = rootLayer;
        Owner?.NodesNeedingPaint.Add(this);
    }

    private void ScheduleInitialLayout()
    {
        RelayoutBoundary = this;
        Owner?.NodesNeedingLayout.Add(this);
    }

    public bool HitTest(HitTestResult result, Offset position)
    {
        Child?.HitTest(result, position);

        result.Add(new HitTestEntry(this));

        return true;
    }

    public override void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        // Do nothing
    }
}
