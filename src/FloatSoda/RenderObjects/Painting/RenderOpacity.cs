using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// 子の描画へ固定の不透明度を適用するRenderObjectです。
/// </summary>
public class RenderOpacity : RenderProxyBox
{
    /// <summary>
    /// 子へ適用する0から1までの不透明度を取得または設定します。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 値が0から1の範囲外、または有限値ではありません。
    /// </exception>
    public double Opacity
    {
        get;
        set
        {
            if (!double.IsFinite(value) || value is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "不透明度には0から1までの有限値を指定してください。");
            }

            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = 1;

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null || Opacity == 0)
        {
            Layer = null;
            return;
        }

        if (Opacity == 1)
        {
            Layer = null;
            base.Paint(context, offset);
            return;
        }

        var alpha = (byte)Math.Round(Opacity * byte.MaxValue);
        Layer = context.PushOpacity(offset, alpha, base.Paint, Layer as OpacityLayer);
    }
}
