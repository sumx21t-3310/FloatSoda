using FloatSoda.Geometrics;
using FloatSoda.Core.Providers;
using RichTextKitStyle = Topten.RichTextKit.Style;

namespace FloatSoda.Painting;

/// <summary>
/// テキストのフォントと描画方法を表す不変の書式です。
/// </summary>
public sealed record TextStyle
{
    /// <summary>フォントサイズを取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下、非数、または無限大です。</exception>
    public double FontSize
    {
        get;
        init
        {
            if (value <= 0 || !double.IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(nameof(FontSize), value, "フォントサイズは0より大きい有限値で指定してください。");
            }

            field = value;
        }
    } = 30;

    /// <summary>テキストの色を取得します。</summary>
    public Color Color { get; init; } = new(0, 0, 0);

    /// <summary>テキスト描画に使用するフォントを取得します。</summary>
    /// <exception cref="ArgumentNullException">値が<see langword="null"/>です。</exception>
    public FontProvider Font
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new SystemFontProvider("Arial");

    /// <summary>フォントの太さを1から1000までの数値で取得します。400が標準、700が太字です。</summary>
    /// <exception cref="ArgumentOutOfRangeException">値が1未満または1000を上回っています。</exception>
    public int FontWeight
    {
        get;
        init
        {
            if (value is < 1 or > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(FontWeight), value, "フォントの太さは1から1000までで指定してください。");
            }

            field = value;
        }
    } = 400;

    /// <summary>斜体で描画するかどうかを取得します。</summary>
    public bool IsItalic { get; init; }

    internal RichTextKitStyle ToRichTextKitStyle() => new()
    {
        FontSize = (float)FontSize,
        TextColor = Color,
        FontFamily = FontResolver.Shared.Register(Font),
        FontWeight = FontWeight,
        FontItalic = IsItalic
    };
}
