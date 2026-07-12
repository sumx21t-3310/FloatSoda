using SkiaSharp;

namespace FloatSoda.UI.Cream;

/// <summary>
/// Cream デザインシステムにおけるボタンの見た目属性。
/// レトロでクリーミーな配色のフラットデザインを既定とする。
/// </summary>
public record ButtonStyle
{
    /// <summary>通常時の背景色。</summary>
    public SKColor BackgroundColor { get; init; } = new(0xFF, 0xF2, 0xCC);

    /// <summary>押下中の背景色。</summary>
    public SKColor PressedBackgroundColor { get; init; } = new(0xF5, 0xDE, 0xA3);

    /// <summary>無効時の背景色。</summary>
    public SKColor DisabledBackgroundColor { get; init; } = new(0xE8, 0xE2, 0xD4);

    /// <summary>既定のスタイル。</summary>
    public static ButtonStyle Default { get; } = new();
}
