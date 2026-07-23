using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// 描画内容と合成処理から成るレイヤーツリーの要素を表します。
/// </summary>
/// <remarks>
/// レイアウトで描画境界を確定した後、同じコンテキストを使用して描画します。
/// </remarks>
public interface ILayer
{
    /// <summary>
    /// レイアウト後にこのレイヤーが描画へ影響する境界を取得します。
    /// </summary>
    /// <value>
    /// 親レイヤーの座標系で表した描画境界。
    /// </value>
    public SKRect PaintBounds { get; }

    /// <summary>
    /// このレイヤーと子レイヤーの描画境界を計算します。
    /// </summary>
    /// <param name="context">
    /// 描画先の座標系とキャンバスを保持するコンテキスト。
    /// </param>
    void Layout(LayerContext context);
    /// <summary>
    /// レイアウト済みの内容を指定された描画先へ合成します。
    /// </summary>
    /// <param name="context">
    /// 合成先のキャンバスを保持するコンテキスト。
    /// </param>
    void Paint(LayerContext context);
    /// <summary>
    /// 現在の合成設定を保持する、独立したレイヤーツリーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを含めて複製された新しいレイヤー。
    /// </returns>
    /// <remarks>
    /// 描画リソースの所有権は移転せず、複製元と共有される場合があります。
    /// </remarks>
    ILayer Clone();
}