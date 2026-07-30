using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>
/// 子の周囲に内側の余白を確保してレイアウトするRenderObjectです。
/// </summary>
public class RenderPadding : RenderProxyBox
{
    private EdgeInsets _padding;

    /// <summary>
    /// 子の周囲に適用する余白を取得または設定します。
    /// </summary>
    /// <value>各辺へ適用する0以上の論理ピクセル単位の余白。</value>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子の制約、位置、および自身のサイズを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの辺が負数または有限値ではありません。</exception>
    public EdgeInsets Padding
    {
        get => _padding;
        set
        {
            Validate(value);

            if (_padding == value) return;

            _padding = value;
            MarkNeedsLayout();
        }
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var horizontal = Padding.Left + Padding.Right;
        var vertical = Padding.Top + Padding.Bottom;

        if (Child is not null)
        {
            Child.Layout(Deflate(Constraints, horizontal, vertical), parentUseSize: true);
            Size = Constraints.Constrain(Child.Size.Width + horizontal, Child.Size.Height + vertical);

            if (Child.ParentData is BoxParentData childParentData)
            {
                childParentData.Offset = Padding.TopLeft;
            }

            return;
        }

        Size = Constraints.Constrain(horizontal, vertical);
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null) return;

        var childParentData = Child.ParentData as BoxParentData;
        context.PaintChild(Child, offset + (childParentData?.Offset ?? Offset.Zero));
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (Child is null) return false;

        var childParentData = Child.ParentData as BoxParentData;
        return result.AddWidthPaintOffset(
            childParentData?.Offset,
            position,
            (testResult, transformed) => Child.HitTest(testResult, transformed));
    }

    private static BoxConstraints Deflate(BoxConstraints constraints, double horizontal, double vertical)
    {
        return new BoxConstraints(
            MinWidth: Math.Max(0, constraints.MinWidth - horizontal),
            MaxWidth: Math.Max(0, constraints.MaxWidth - horizontal),
            MinHeight: Math.Max(0, constraints.MinHeight - vertical),
            MaxHeight: Math.Max(0, constraints.MaxHeight - vertical));
    }

    private static void Validate(EdgeInsets padding)
    {
        if (IsFiniteAndNonNegative(padding.Left)
            && IsFiniteAndNonNegative(padding.Top)
            && IsFiniteAndNonNegative(padding.Right)
            && IsFiniteAndNonNegative(padding.Bottom))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(padding),
            padding,
            "Padding must contain only finite, non-negative values.");
    }

    private static bool IsFiniteAndNonNegative(double value) => double.IsFinite(value) && value >= 0;
}
