using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using static System.Double;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>
/// 自身の領域内で子を指定した配置に位置決めするRenderObjectです。
/// </summary>
public class RenderPositionedBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderObject> _child;

    /// <summary>
    /// 子を持たない位置決め用RenderObjectを初期化します。
    /// </summary>
    public RenderPositionedBox() => _child = new SingleChildContainer<RenderObject>(this);

    /// <summary>
    /// 配置する子を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 子を差し替えると、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に自身のサイズと子の位置を再計算します。
    /// 同じ子を再設定した場合も、子の取り外しと追加が行われるためLayout Dirtyとなります。
    /// </remarks>
    public RenderObject? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    /// <summary>
    /// 子の幅に乗算して自身の幅を決める係数を取得または設定します。
    /// </summary>
    /// <remarks>
    /// <c>null</c>の場合、有限の親制約では利用可能な最大幅を使用します。
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に自身のサイズと子の位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public double? WidthFactor
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
    /// 子の高さに乗算して自身の高さを決める係数を取得または設定します。
    /// </summary>
    /// <remarks>
    /// <c>null</c>の場合、有限の親制約では利用可能な最大高さを使用します。
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に自身のサイズと子の位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public double? HeightFactor
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
    /// 自身の領域内で子を配置する位置を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子のオフセットを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public Alignment Alignment
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            MarkNeedsLayout();
        }
    } = default;


    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var shrinkWrapWidth = WidthFactor.HasValue || IsPositiveInfinity(Constraints.MaxWidth);
        var shrinkWrapHeight = HeightFactor.HasValue || IsPositiveInfinity(Constraints.MaxHeight);

        if (Child != null)
        {
            Child.ParentData ??= new BoxParentData();
            Child.Layout(Constraints.Loosen, parentUseSize: true);

            Size = Constraints.Constrain(
                width: shrinkWrapWidth ? Child.Size.Width * (WidthFactor ?? 1) : PositiveInfinity,
                height: shrinkWrapHeight ? Child.Size.Height * (HeightFactor ?? 1) : PositiveInfinity
            );

            AlignChild();
        }
        else
        {
            Size = Constraints.Constrain(
                width: shrinkWrapWidth ? 0 : PositiveInfinity,
                height: shrinkWrapHeight ? 0 : PositiveInfinity
            );
        }
    }

    private void AlignChild()
    {
        if (Child?.ParentData is not BoxParentData childParentData) return;
        var offset = Alignment.ComputeOffset(Size, Child.Size);
        childParentData.Offset = offset;
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child == null) return;

        var childParentData = Child.ParentData as BoxParentData;
        context.PaintChild(Child, offset + (childParentData?.Offset ?? Offset.Zero));
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

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (Child is null) return false;

        var childParentData = Child.ParentData as BoxParentData;

        return result.AddWidthPaintOffset(
            childParentData?.Offset,
            position,
            (testResult, transformed) => (Child as RenderBox)?.HitTest(testResult, transformed) ?? false
        );
    }
}
