using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>
/// 親制約を任意に変換して子へ渡し、親制約内で子を配置するRenderObjectです。
/// </summary>
public sealed class RenderConstraintsTransformBox : RenderProxyBox
{
    private bool _hasOverflow;

    /// <summary>子へ渡す制約を生成する変換を取得または設定します。</summary>
    /// <exception cref="ArgumentNullException">値が<see langword="null"/>です。</exception>
    public required BoxConstraintsTransform ConstraintsTransform
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>自身と子のサイズが異なる場合に使用する配置を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が非有限値です。</exception>
    public Alignment Alignment
    {
        get;
        set
        {
            ValidateAlignment(value);
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = Alignment.Center;

    /// <summary>子が自身の領域からはみ出す場合の切り抜き方法を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public Clip ClipBehavior
    {
        get;
        set
        {
            ValidateClipBehavior(value);
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = Clip.None;

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Child is null)
        {
            Size = Constraints.Smallest;
            _hasOverflow = false;
            return;
        }

        Child.Layout(TransformAndValidate(Constraints), parentUseSize: true);
        Size = Constraints.Constrain(Child.Size);

        var childParentData = (BoxParentData)Child.ParentData!;
        childParentData.Offset = Alignment.ComputeOffset(Size, Child.Size);
        _hasOverflow = IsOutsideBounds(childParentData.Offset, Child.Size, Size);
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        if (Child is null) return constraints.Smallest;

        var childSize = Child.GetDryLayout(TransformAndValidate(constraints));
        return constraints.Constrain(childSize);
    }

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => Child is null
        ? 0
        : Child.GetMinIntrinsicWidth(TransformAndValidate(new BoxConstraints(MaxHeight: height)).MaxHeight);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => Child is null
        ? 0
        : Child.GetMaxIntrinsicWidth(TransformAndValidate(new BoxConstraints(MaxHeight: height)).MaxHeight);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => Child is null
        ? 0
        : Child.GetMinIntrinsicHeight(TransformAndValidate(new BoxConstraints(MaxWidth: width)).MaxWidth);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => Child is null
        ? 0
        : Child.GetMaxIntrinsicHeight(TransformAndValidate(new BoxConstraints(MaxWidth: width)).MaxWidth);

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null)
        {
            Layer = null;
            return;
        }

        var childOffset = ((BoxParentData)Child.ParentData!).Offset;
        if (!_hasOverflow || ClipBehavior == Clip.None)
        {
            Layer = null;
            context.PaintChild(Child, offset + childOffset);
            return;
        }

        Layer = context.PushClipRect(
            offset,
            SKRect.Create(Size),
            (childContext, paintOffset) => childContext.PaintChild(Child, paintOffset + childOffset),
            ClipBehavior,
            Layer as ClipRectLayer);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (Child is null) return false;

        var childOffset = ((BoxParentData)Child.ParentData!).Offset;
        return result.AddWidthPaintOffset(
            childOffset,
            position,
            (testResult, transformed) => Child.HitTest(testResult, transformed));
    }

    private BoxConstraints TransformAndValidate(BoxConstraints constraints)
    {
        var transformed = ConstraintsTransform(constraints);
        ValidateConstraints(transformed);
        return transformed;
    }

    private static bool IsOutsideBounds(Offset offset, SKSize childSize, SKSize size) =>
        offset.X < 0 || offset.Y < 0
        || offset.X + childSize.Width > size.Width
        || offset.Y + childSize.Height > size.Height;

    private static void ValidateConstraints(BoxConstraints constraints)
    {
        ValidateAxis(constraints.MinWidth, constraints.MaxWidth, nameof(constraints.MinWidth));
        ValidateAxis(constraints.MinHeight, constraints.MaxHeight, nameof(constraints.MinHeight));
    }

    private static void ValidateAxis(double minimum, double maximum, string parameterName)
    {
        if (!double.IsFinite(minimum) || minimum < 0
            || double.IsNaN(maximum) || double.IsNegativeInfinity(maximum)
            || maximum < minimum)
        {
            throw new ArgumentException(
                "制約変換の戻り値は、0以上の有限な最小値と、それ以上の最大値または正の無限大を持つ必要があります。",
                parameterName);
        }
    }

    private static void ValidateAlignment(Alignment alignment)
    {
        if (!float.IsFinite(alignment.X) || !float.IsFinite(alignment.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "配置値には有限値を指定してください。");
        }
    }

    private static void ValidateClipBehavior(Clip clipBehavior)
    {
        if (!Enum.IsDefined(clipBehavior))
        {
            throw new ArgumentOutOfRangeException(nameof(clipBehavior), clipBehavior, "定義済みのクリップ方法を指定してください。");
        }
    }
}
