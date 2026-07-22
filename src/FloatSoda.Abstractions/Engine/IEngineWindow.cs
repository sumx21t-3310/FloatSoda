using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;

namespace FloatSoda.Abstractions.Engine;

public interface IEngineWindow : IDisposable
{
    /// <summary>
    /// 指定されたレイヤーツリーをウィンドウの描画先へ反映します。
    /// </summary>
    void Present(ILayer layer);

    /// <summary>
    /// 描画先テクスチャ／サーフェスのサイズを変更します。レンダースレッド上で呼ぶ必要があります。
    /// </summary>
    void Resize(SkiaSharp.SKSizeI size);

    /// <summary>
    /// このウィンドウの生ポインタ入力源。入力を提供しないウィンドウでは null。
    /// </summary>
    IRawPointerSource? PointerSource { get; }
}
