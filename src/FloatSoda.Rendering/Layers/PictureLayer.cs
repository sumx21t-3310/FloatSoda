using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// 記録済みの描画命令を再生するレイヤーです。
/// </summary>
/// <seealso cref="ContainerLayer"/>
public class PictureLayer : ILayer
{
    /// <summary>
    /// 再生する記録済みの描画命令を取得または設定します。
    /// </summary>
    /// <value>
    /// 描画するピクチャー。<see langword="null"/>の場合は何も描画しません。
    /// 所有権はこのレイヤーへ移転せず、使用中は呼び出し元が有効な状態に保つ必要があります。
    /// </value>
    public SKPicture? Picture { get; set; }

    /// <summary>
    /// レイアウト後のピクチャーの描画境界を取得します。
    /// </summary>
    /// <value>
    /// ピクチャーに記録された描画境界。ピクチャーが<see langword="null"/>の場合は空の矩形。
    /// </value>
    public SKRect PaintBounds { get; private set; }


    /// <summary>
    /// ピクチャーに記録された描画境界をこのレイヤーの描画境界として設定します。
    /// </summary>
    /// <param name="context">
    /// レイヤーツリーのレイアウトに使用するコンテキスト。
    /// </param>
    public void Layout(LayerContext context)
    {
        PaintBounds = Picture?.CullRect ?? new SKRect();
    }

    /// <summary>
    /// 記録済みの描画命令を指定されたキャンバスへ再生します。
    /// </summary>
    /// <param name="context">
    /// 描画命令の再生先となるキャンバスを保持するコンテキスト。
    /// </param>
    public void Paint(LayerContext context)
    {
        Picture?.Playback(context.Canvas);
    }

    /// <summary>
    /// 同じピクチャーを参照する新しいピクチャーレイヤーを作成します。
    /// </summary>
    /// <returns>
    /// ピクチャーへの参照を共有する新しいピクチャーレイヤー。
    /// </returns>
    /// <remarks>
    /// ピクチャーの所有権は移転しません。描画境界は複製後のレイアウトで計算されます。
    /// </remarks>
    public ILayer Clone() => new PictureLayer()
    {
        Picture = Picture
    };
}