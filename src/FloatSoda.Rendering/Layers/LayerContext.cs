using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// レイヤーツリーのレイアウトと描画で共有する描画先を表します。
/// </summary>
/// <param name="Canvas">
/// レイヤーの描画先となるキャンバス。所有権は呼び出し元に残ります。
/// </param>
public readonly record struct LayerContext(SKCanvas Canvas)
{
    /// <summary>
    /// サーフェスのキャンバスを描画先とするコンテキストを作成します。
    /// </summary>
    /// <param name="surface">
    /// 描画先のサーフェス。所有権は呼び出し元に残り、コンテキストの使用中は有効な状態に保つ必要があります。
    /// </param>
    /// <returns>
    /// 指定されたサーフェスのキャンバスを参照するコンテキスト。
    /// </returns>
    public static LayerContext Create(SKSurface surface) => new(surface.Canvas);
}