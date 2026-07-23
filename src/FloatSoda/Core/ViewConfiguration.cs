using SkiaSharp;

namespace FloatSoda.Core;

/// <summary>描画対象ビューのピクセル単位の大きさを表す不変の構成値です。</summary>
/// <param name="Size">ビューの幅と高さ。単位はピクセルです。</param>
public readonly record struct ViewConfiguration(SKSize Size);