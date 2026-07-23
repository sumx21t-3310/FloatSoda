using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// 自身の領域を単色で塗りつぶし、その上に子を描画するRenderObjectです。
/// </summary>
public class RenderColoredBox : RenderProxyBox
{
    /// <summary>
    /// 背景の塗りつぶし色を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをPaint Dirtyとしてマークし、
    /// 次のパイプライン更新時に背景と子を再描画します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public SKColor Color
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = SKColors.Black;

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Size.IsEmpty)
        {
            context.Canvas.DrawRect(SKRect.Create(offset, Size), new SKPaint { Color = Color });
        }

        if (Child is not null) context.PaintChild(Child, offset);
    }


    /// <inheritdoc/>
    public override bool HitTest(HitTestResult result, Offset position)
    {
        if (!Size.Contains(position)) return false;
        
        var hitTarget = HitTestChildren(result, position) || HitTestSelf(position);

        if (hitTarget)
        {
            result.Add(new HitTestEntry(this));
        }

        return hitTarget;
    }

    /// <inheritdoc/>
    public override bool HitTestSelf(Offset position) => true;
}
