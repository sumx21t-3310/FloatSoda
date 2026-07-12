using SkiaSharp;

namespace FloatSoda.UI.FizzyPop;

/// <summary>
/// FizzyPop デザインシステムにおけるボタンの見た目属性。
/// 半透明ホワイトを基調とした透明感のあるデザインを既定とする。
/// </summary>
public record ButtonStyle
{
    /// <summary>通常時の背景色。</summary>
    public SKColor BackgroundColor { get; init; } = new(0xFF, 0xFF, 0xFF, 0x59);

    /// <summary>押下中の背景色。</summary>
    public SKColor PressedBackgroundColor { get; init; } = new(0xFF, 0xFF, 0xFF, 0x8C);

    /// <summary>無効時の背景色。</summary>
    public SKColor DisabledBackgroundColor { get; init; } = new(0xFF, 0xFF, 0xFF, 0x26);

    /// <summary>既定のスタイル。</summary>
    public static ButtonStyle Default { get; } = new();
}
