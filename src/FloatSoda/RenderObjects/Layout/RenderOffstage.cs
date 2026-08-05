using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>子をレイアウトしたまま、必要に応じて描画とヒットテストから除外するRenderObjectです。</summary>
public class RenderOffstage : RenderProxyBox
{
    /// <summary>子を画面外として扱うかを取得または設定します。</summary>
    public bool Offstage
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = true;

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Child is null)
        {
            Size = Constraints.Smallest;
            return;
        }

        Child.Layout(Constraints, parentUseSize: !Offstage);
        Size = Offstage ? Constraints.Smallest : Child.Size;
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Offstage) base.Paint(context, offset);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
        => !Offstage && base.HitTestChildren(result, position);
}
