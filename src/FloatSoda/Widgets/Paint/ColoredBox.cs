using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// 自身の領域を単色で塗りつぶし、その上に子要素を描画します。
/// </summary>
/// <seealso cref="RenderColoredBox"/>
public record ColoredBox : SingleChildRenderObjectWidget<RenderColoredBox>
{
    /// <summary>
    /// 背景の塗りつぶしに使用する色を取得します。
    /// </summary>
    public Color Color { get; init; }
    
    /// <summary>
    /// このウィジェットの背景色を描画するRenderObjectを生成します。
    /// </summary>
    /// <returns>指定された背景色を保持する新しいRenderObject。</returns>
    public override RenderColoredBox CreateRenderObject()
    {
        return new RenderColoredBox
        {
            Color = Color
        };
    }

    /// <summary>
    /// 背景色を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// 色が変更された場合、対象をPaint Dirtyとしてマークし、
    /// 次のパイプライン更新時に背景を再描画します。
    /// 色が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderColoredBox renderObject) => renderObject.Color = Color;
}