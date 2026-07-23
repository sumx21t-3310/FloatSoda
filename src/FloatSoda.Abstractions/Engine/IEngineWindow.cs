using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;

namespace FloatSoda.Abstractions.Engine;

/// <summary>レイヤーツリーの描画先と、その描画先に関連付けられたポインター入力を表します。</summary>
/// <seealso cref="ILayer"/>
/// <seealso cref="IRawPointerSource"/>
public interface IEngineWindow : IDisposable
{
    /// <summary>
    /// 指定されたレイヤーツリーをウィンドウの描画先へ反映します。
    /// </summary>
    /// <param name="layer">反映するレイヤーツリー。<see langword="null"/>は指定できません。</param>
    /// <remarks>描画先を所有するレンダースレッド上で呼び出します。</remarks>
    void Present(ILayer layer);

    /// <summary>
    /// 描画先テクスチャ／サーフェスのサイズを変更します。レンダースレッド上で呼ぶ必要があります。
    /// </summary>
    /// <param name="size">変更後のピクセルサイズ。幅と高さには0より大きい値を指定します。</param>
    void Resize(SkiaSharp.SKSizeI size);

    /// <summary>
    /// このウィンドウの生ポインタ入力源。入力を提供しないウィンドウでは null。
    /// </summary>
    IRawPointerSource? PointerSource { get; }
}
