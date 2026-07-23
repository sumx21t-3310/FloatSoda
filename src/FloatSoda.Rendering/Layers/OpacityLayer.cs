using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// すべての子レイヤーをひとまとまりとして、指定された不透明度で合成するレイヤーです。
/// </summary>
public class OpacityLayer : ContainerLayer
{
    /// <summary>
    /// 子レイヤーの合成に適用する不透明度を取得または設定します。
    /// </summary>
    /// <value>
    /// 0を完全な透明、255を完全な不透明とする値。既定値は255です。
    /// </value>
    public byte Alpha { get; set; } = 255;

    /// <summary>
    /// 子レイヤーを一時レイヤーへ描画し、指定された不透明度で描画先へ合成します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーの合成先となるキャンバスを保持するコンテキスト。
    /// </param>
    /// <remarks>
    /// 合成後にキャンバスの状態を復元するため、後続のレイヤーへ不透明度は引き継がれません。
    /// </remarks>
    public override void Paint(LayerContext context)
    {
        var paint = new SKPaint()
        {
            Color = SKColors.White.WithAlpha(Alpha),
            IsAntialias = true
        };

        context.Canvas.SaveLayer(paint);

        base.Paint(context);

        context.Canvas.Restore();
    }

    /// <summary>
    /// 不透明度と子レイヤーを保持する新しい不透明度レイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを再帰的に複製した新しい不透明度レイヤー。
    /// </returns>
    public override ILayer Clone()
    {
        var cloned = new OpacityLayer { Alpha = Alpha };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}