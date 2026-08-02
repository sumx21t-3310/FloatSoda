using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>Stackが子ごとに保持する絶対配置情報です。</summary>
public class StackParentData : BoxParentData
{
    /// <summary>Stack左端から子の左端までの距離を取得または設定します。</summary>
    public double? Left { get; set; }

    /// <summary>Stack上端から子の上端までの距離を取得または設定します。</summary>
    public double? Top { get; set; }

    /// <summary>Stack右端から子の右端までの距離を取得または設定します。</summary>
    public double? Right { get; set; }

    /// <summary>Stack下端から子の下端までの距離を取得または設定します。</summary>
    public double? Bottom { get; set; }

    /// <summary>子へ要求する幅を取得または設定します。</summary>
    public double? Width { get; set; }

    /// <summary>子へ要求する高さを取得または設定します。</summary>
    public double? Height { get; set; }

    /// <summary>いずれかの絶対配置値が指定されているかを取得します。</summary>
    public bool IsPositioned => Left.HasValue || Top.HasValue || Right.HasValue || Bottom.HasValue || Width.HasValue || Height.HasValue;
}

/// <summary>複数の子を重ね、通常配置または絶対配置で位置決めするRenderObjectです。</summary>
public class RenderStack : RenderBox, IHasMultiChildrenRenderObject
{
    /// <summary>レイアウト対象となる子のコレクションを取得します。</summary>
    public MultiChildrenCollection<RenderBox> Children { get; }

    /// <summary>子を持たないStackレイアウトを初期化します。</summary>
    public RenderStack() => Children = new MultiChildrenCollection<RenderBox>(this);

    /// <summary>非Positioned子と未指定軸のPositioned子を配置する基準位置を取得または設定します。</summary>
    public Alignment Alignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = Alignment.TopLeft;

    /// <summary>非Positioned子へ渡す制約の方式を取得または設定します。</summary>
    public StackFit Fit
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = StackFit.Loose;

    /// <summary>指定したRenderObjectを末尾の子として追加します。</summary>
    /// <param name="child">追加するRenderObject。<see cref="RenderBox"/>である必要があります。</param>
    public void AddChild(RenderObject child) => Children.Add((RenderBox)child);

    /// <summary>指定したRenderObjectを子のコレクションから削除します。</summary>
    /// <param name="child">削除するRenderObject。</param>
    /// <returns>子が見つかり削除された場合は<see langword="true"/>。</returns>
    public bool RemoveChild(RenderObject child) => Children.Remove((RenderBox)child);

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new StackParentData();

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var nonPositionedConstraints = GetNonPositionedConstraints();
        var hasNonPositionedChild = false;
        double width = 0;
        double height = 0;

        foreach (var child in Children)
        {
            var parentData = (StackParentData)child.ParentData!;
            if (parentData.IsPositioned) continue;

            hasNonPositionedChild = true;
            child.Layout(nonPositionedConstraints, parentUseSize: true);
            width = Math.Max(width, child.Size.Width);
            height = Math.Max(height, child.Size.Height);
        }

        Size = hasNonPositionedChild
            ? Constraints.Constrain(width, height)
            : Constraints.Constrain(
                double.IsFinite(Constraints.MaxWidth) ? Constraints.MaxWidth : Constraints.MinWidth,
                double.IsFinite(Constraints.MaxHeight) ? Constraints.MaxHeight : Constraints.MinHeight);

        foreach (var child in Children)
        {
            var parentData = (StackParentData)child.ParentData!;
            if (parentData.IsPositioned)
            {
                LayoutPositionedChild(child, parentData);
            }
            else
            {
                parentData.Offset = Alignment.ComputeOffset(Size, child.Size);
            }
        }
    }

    private BoxConstraints GetNonPositionedConstraints() => Fit switch
    {
        StackFit.Loose => Constraints.Loosen,
        StackFit.Expand => new BoxConstraints(
            MinWidth: double.IsFinite(Constraints.MaxWidth) ? Constraints.MaxWidth : Constraints.MinWidth,
            MaxWidth: Constraints.MaxWidth,
            MinHeight: double.IsFinite(Constraints.MaxHeight) ? Constraints.MaxHeight : Constraints.MinHeight,
            MaxHeight: Constraints.MaxHeight),
        StackFit.Passthrough => Constraints,
        _ => throw new ArgumentOutOfRangeException(nameof(Fit), Fit, null),
    };

    private void LayoutPositionedChild(RenderBox child, StackParentData parentData)
    {
        var width = parentData.Left.HasValue && parentData.Right.HasValue
            ? Math.Max(0, Size.Width - parentData.Left.Value - parentData.Right.Value)
            : parentData.Width;
        var height = parentData.Top.HasValue && parentData.Bottom.HasValue
            ? Math.Max(0, Size.Height - parentData.Top.Value - parentData.Bottom.Value)
            : parentData.Height;

        child.Layout(BoxConstraints.TightFor(width, height), parentUseSize: true);

        var alignedOffset = Alignment.ComputeOffset(Size, child.Size);
        var x = parentData.Left
            ?? (parentData.Right.HasValue ? Size.Width - parentData.Right.Value - child.Size.Width : alignedOffset.X);
        var y = parentData.Top
            ?? (parentData.Bottom.HasValue ? Size.Height - parentData.Bottom.Value - child.Size.Height : alignedOffset.Y);

        parentData.Offset = new Offset((float)x, (float)y);
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        foreach (var child in Children)
        {
            var parentData = (StackParentData)child.ParentData!;
            context.PaintChild(child, offset + parentData.Offset);
        }
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        foreach (var child in Children.Reverse())
        {
            var parentData = (StackParentData)child.ParentData!;
            if (result.AddWidthPaintOffset(
                    parentData.Offset,
                    position,
                    (testResult, transformed) => child.HitTest(testResult, transformed)))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        Children.Attach(owner);
    }

    /// <inheritdoc/>
    public override void Detach()
    {
        base.Detach();
        Children.Detach();
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<RenderObject> visitor) => Children.VisitChildren(visitor);

    /// <inheritdoc/>
    public override void RedepthChildren() => VisitChildren(RedepthChild);
}
