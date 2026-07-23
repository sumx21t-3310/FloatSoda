using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Engine;

/// <summary>
/// レイヤーツリーを <see cref="GLView"/> の Skia サーフェスへ描画します。
/// </summary>
public class Renderer : IDisposable
{
    /// <summary>
    /// 描画先と OpenGL リソースを所有するビューを取得します。
    /// </summary>
    public required GLView GLView { get; init; }

    /// <summary>
    /// 描画結果を保持する OpenGL テクスチャのネイティブハンドルを取得します。
    /// </summary>
    /// <returns>OpenGL テクスチャ名をポインター値へ変換したハンドル。</returns>
    public IntPtr GetTextureHandle() => GLView.TextureHandle;

    /// <summary>
    /// 描画先を透明色でクリアし、指定したレイヤーツリーを描画してコマンドを送信します。
    /// </summary>
    /// <param name="root">描画するレイヤーツリーのルート。</param>
    public void Render(ILayer root)
    {
        var renderContext = LayerContext.Create(GLView.Surface);

        GLView.Clear();

        LayerRenderer.Render(root, renderContext);

        GLView.Flush();
    }

    /// <summary>描画先の GLView を指定サイズにリサイズします。GL 呼び出しのためレンダースレッド上で呼びます。</summary>
    /// <param name="width">新しい描画先の幅（ピクセル）。</param>
    /// <param name="height">新しい描画先の高さ（ピクセル）。</param>
    public void Resize(int width, int height) => GLView.Resize(width, height);

    /// <summary>
    /// 描画先の GLView を指定サイズにリサイズします。GL 呼び出しのためレンダースレッド上で呼びます。
    /// </summary>
    /// <param name="size">新しい描画先のピクセルサイズ。</param>
    public void Resize(SKSizeI size) => GLView.Resize(size);

    /// <summary>
    /// 描画先の <see cref="GLView"/> と、その Skia および OpenGL リソースを解放します。
    /// </summary>
    public void Dispose() => GLView.Dispose();
}
