using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>指定サイズを自身へ適用し、親から受け取った元の制約で子をレイアウトするRenderObjectです。</summary>
public sealed class RenderSizedOverflowBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderObject> _child;

    /// <summary>子を持たない固定サイズoverflow用RenderObjectを初期化します。</summary>
    public RenderSizedOverflowBox() => _child = new SingleChildContainer<RenderObject>(this);

    /// <summary>配置する子を取得または設定します。</summary>
    public RenderObject? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    /// <summary>自身が採ろうとするサイズを取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">幅または高さが負数または非有限値です。</exception>
    public Geometrics.Size RequestedSize
    {
        get;
        set
        {
            ValidateSize(value, nameof(RequestedSize));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>自身の領域内で子を配置する位置を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が非有限値です。</exception>
    public Alignment Alignment
    {
        get;
        set
        {
            LayoutOverflowValidation.ValidateAlignment(value, nameof(Alignment));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = Alignment.Center;

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        Size = Constraints.Constrain(RequestedSize.Width, RequestedSize.Height);
        if (Child is null) return;

        Child.Layout(Constraints, parentUseSize: true);
        AlignChild();
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints) =>
        constraints.Constrain(RequestedSize.Width, RequestedSize.Height);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => RequestedSize.Width;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => RequestedSize.Width;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => RequestedSize.Height;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => RequestedSize.Height;

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null) return;
        var childOffset = ((BoxParentData)Child.ParentData!).Offset;
        context.PaintChild(Child, offset + childOffset);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (Child is not RenderBox child) return false;
        var childOffset = ((BoxParentData)Child.ParentData!).Offset;
        return result.AddWidthPaintOffset(childOffset, position,
            (testResult, transformed) => child.HitTest(testResult, transformed));
    }

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    /// <inheritdoc/>
    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    /// <inheritdoc/>
    public override void RedepthChildren() => VisitChildren(RedepthChild);

    internal static void ValidateSize(Geometrics.Size value, string parameterName)
    {
        if (!double.IsFinite(value.Width) || value.Width < 0
            || !double.IsFinite(value.Height) || value.Height < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "サイズの幅と高さには0以上の有限値を指定してください。");
        }
    }

    private void AlignChild() => ((BoxParentData)Child!.ParentData!).Offset = Alignment.ComputeOffset(Size, Child.Size);
}
