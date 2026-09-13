using FloatSoda.Geometrics;
using FloatSoda.Core.Providers;
using RichTextKitStyle = Topten.RichTextKit.Style;

namespace FloatSoda.Painting;

/// <summary>
/// テキストのフォントと描画方法を表す不変の書式です。
/// </summary>
/// <remarks>
/// 各プロパティは<see langword="null"/>を「未指定」として扱います。
/// 未指定の値は<see cref="Merge"/>による継承で埋められ、
/// 描画時まで残った未指定の値にはフレームワークの既定値が適用されます。
/// </remarks>
public sealed record TextStyle
{
    internal const double DefaultFontSize = 30;
    internal static readonly Color DefaultColor = new(0, 0, 0);
    internal static readonly FontProvider DefaultFont = new SystemFontProvider("Arial");
    internal const int DefaultFontWeight = 400;

    /// <summary>
    /// 未指定のプロパティを祖先の書式から継承するかどうかを取得します。
    /// </summary>
    /// <remarks>
    /// <see langword="false"/>の場合、<see cref="Merge"/>の対象になってもこの書式がそのまま採用され、
    /// 未指定のプロパティには既定値が適用されます。
    /// </remarks>
    public bool Inherit { get; init; } = true;

    /// <summary>フォントサイズを取得します。</summary>
    /// <remarks><see langword="null"/>の場合は継承元の値、継承元が無ければ既定値(30)を使用します。</remarks>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下、非数、または無限大です。</exception>
    public double? FontSize
    {
        get;
        init
        {
            if (value is { } size && (size <= 0 || !double.IsFinite(size)))
            {
                throw new ArgumentOutOfRangeException(nameof(FontSize), value, "フォントサイズは0より大きい有限値で指定してください。");
            }

            field = value;
        }
    }

    /// <summary>テキストの色を取得します。</summary>
    /// <remarks><see langword="null"/>の場合は継承元の値、継承元が無ければ既定値(黒)を使用します。</remarks>
    public Color? Color { get; init; }

    /// <summary>テキスト描画に使用するフォントを取得します。</summary>
    /// <remarks><see langword="null"/>の場合は継承元の値、継承元が無ければ既定値(システムのArial)を使用します。</remarks>
    public FontProvider? Font { get; init; }

    /// <summary>フォントの太さを1から1000までの数値で取得します。400が標準、700が太字です。</summary>
    /// <remarks><see langword="null"/>の場合は継承元の値、継承元が無ければ既定値(400)を使用します。</remarks>
    /// <exception cref="ArgumentOutOfRangeException">値が1未満または1000を上回っています。</exception>
    public int? FontWeight
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
    }

    /// <summary>斜体で描画するかどうかを取得します。</summary>
    /// <remarks><see langword="null"/>の場合は継承元の値、継承元が無ければ既定値(斜体にしない)を使用します。</remarks>
    public bool? IsItalic { get; init; }

    /// <summary>
    /// この書式を基底として、指定した書式で明示されたプロパティを上書きした新しい書式を返します。
    /// </summary>
    /// <param name="other">上書きする書式。<see langword="null"/>の場合はこの書式をそのまま返します。</param>
    /// <returns>
    /// マージ結果の書式。<paramref name="other"/>の<see cref="Inherit"/>が
    /// <see langword="false"/>の場合は<paramref name="other"/>をそのまま返します。
    /// </returns>
    public TextStyle Merge(TextStyle? other)
    {
        if (other is null) return this;
        if (!other.Inherit) return other;

        return this with
        {
            FontSize = other.FontSize ?? FontSize,
            Color = other.Color ?? Color,
            Font = other.Font ?? Font,
            FontWeight = other.FontWeight ?? FontWeight,
            IsItalic = other.IsItalic ?? IsItalic
        };
    }

    internal RichTextKitStyle ToRichTextKitStyle() => new()
    {
        FontSize = (float)(FontSize ?? DefaultFontSize),
        TextColor = Color ?? DefaultColor,
        FontFamily = FontResolver.Shared.Register(Font ?? DefaultFont),
        FontWeight = FontWeight ?? DefaultFontWeight,
        FontItalic = IsItalic ?? false
    };
}
