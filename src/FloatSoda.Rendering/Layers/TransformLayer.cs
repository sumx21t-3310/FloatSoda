using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// すべての子レイヤーへ同じ座標変換を適用して合成するレイヤーです。
/// </summary>
public class TransformLayer : ContainerLayer
{
    /// <summary>
    /// 子レイヤーの座標系へ適用する変換行列を取得または設定します。
    /// </summary>
    /// <value>
    /// レイアウト境界と描画内容の両方へ適用する変換行列。既定値は恒等変換です。
    /// </value>
    public SKMatrix Transform { get; set; } = SKMatrix.Identity;

    /// <summary>
    /// 子レイヤーをレイアウトし、変換後の領域をこのレイヤーの描画境界として設定します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーのレイアウトに使用するコンテキスト。
    /// </param>
    public override void Layout(LayerContext context)
    {
        var childBounds = LayoutChildren(context);

        PaintBounds = Transform.MapRect(childBounds);
    }

    /// <summary>
    /// 変換行列を子レイヤーの座標系へ適用して描画します。
    /// </summary>
    /// <param name="context">
    /// 変換後の子レイヤーを合成するキャンバスを保持するコンテキスト。
    /// </param>
    /// <remarks>
    /// 描画後にキャンバスの状態を復元するため、後続のレイヤーへ変換は引き継がれません。
    /// </remarks>
    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.Concat(Transform);

        base.Paint(context);

        context.Canvas.Restore();
    }

    /// <summary>
    /// 変換行列と子レイヤーを保持する新しい変換レイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを再帰的に複製した新しい変換レイヤー。
    /// </returns>
    public override ILayer Clone()
    {
        var cloned = new TransformLayer()
        {
            Transform = Transform
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}