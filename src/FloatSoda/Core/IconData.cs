using FloatSoda.Core.Providers;

namespace FloatSoda.Core;

/// <summary>アイコンフォント内の一つのグリフを表します。</summary>
public readonly record struct IconData
{
    /// <summary>アイコンのコードポイントとフォントを指定して初期化します。</summary>
    /// <param name="codePoint">描画するUnicodeコードポイント。</param>
    /// <param name="font">グリフを含むフォント。</param>
    public IconData(int codePoint, FontProvider font)
    {
        if (codePoint is < 0 or > 0x10ffff || codePoint is >= 0xd800 and <= 0xdfff)
        {
            throw new ArgumentOutOfRangeException(nameof(codePoint), codePoint, "有効なUnicodeコードポイントを指定してください。");
        }

        ArgumentNullException.ThrowIfNull(font);

        CodePoint = codePoint;
        Font = font;
    }

    /// <summary>描画するUnicodeコードポイントを取得します。</summary>
    public int CodePoint { get; }

    /// <summary>グリフを含むフォントを取得します。</summary>
    public FontProvider Font { get; }
}
