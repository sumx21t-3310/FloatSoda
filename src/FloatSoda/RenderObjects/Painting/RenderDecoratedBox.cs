using FloatSoda.Abstractions.Geometries;
using FloatSoda.Painting;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// 子の前面または背面へボックス装飾を描画するRenderObjectです。
/// </summary>
public class RenderDecoratedBox : RenderProxyBox
{
    /// <summary>
    /// 装飾の描画位置を取得または設定します。
    /// </summary>
    public DecorationPosition Position
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    }

    /// <summary>
    /// 描画するボックス装飾を取得または設定します。
    /// </summary>
    public required BoxDecoration Decoration
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Position == DecorationPosition.Background)
        {
            PaintDecoration(context, offset);
        }

        base.Paint(context, offset);

        if (Position == DecorationPosition.Foreground)
        {
            PaintDecoration(context, offset);
        }
    }

    /// <inheritdoc/>
    public override bool HitTestSelf(Offset position) =>
        Decoration.HitTest(SkiaSharp.SKRect.Create(Offset.Zero, Size), position);

    private void PaintDecoration(PaintingContext context, Offset offset) =>
        Decoration.Paint(context.Canvas, SkiaSharp.SKRect.Create(offset, Size));
}
