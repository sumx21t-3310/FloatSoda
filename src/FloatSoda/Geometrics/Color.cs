using System.Globalization;
using SkiaSharp;

namespace FloatSoda.Geometrics;

/// <summary>赤、緑、青、不透明度の各成分を8ビットで表す不変の色値です。</summary>
/// <param name="Red">赤成分。0から255までで、既定値は255です。</param>
/// <param name="Green">緑成分。0から255までで、既定値は255です。</param>
/// <param name="Blue">青成分。0から255までで、既定値は255です。</param>
/// <param name="Alpha">不透明度。0は完全な透明、255は完全な不透明で、既定値は255です。</param>
/// <seealso cref="HSVColor"/>
/// <seealso cref="HSLColor"/>
public readonly record struct Color(byte Red = 255, byte Green = 255, byte Blue = 255, byte Alpha = 255)
{
    /// <summary>ウェブカラー表記から不透明な色値を作成します。</summary>
    /// <param name="webColor">先頭の番号記号を省略できる3桁または6桁の16進数表記。</param>
    /// <returns>不透明度が255の色値。</returns>
    /// <exception cref="ArgumentException"><paramref name="webColor"/>が<see langword="null"/>、空、または空白だけです。</exception>
    /// <exception cref="FormatException">文字数が3桁または6桁ではないか、16進数以外の文字を含みます。</exception>
    public static Color Parse(string webColor)
    {
        if (string.IsNullOrWhiteSpace(webColor))
        {
            throw new ArgumentException("カラーコードが空です。", nameof(webColor));
        }

        // 先頭の '#' を取り除く
        ReadOnlySpan<char> colorSpan = webColor.AsSpan();
        if (colorSpan[0] == '#')
        {
            colorSpan = colorSpan[1..];
        }

        // #RRGGBB 形式の場合 (文字数が6)
        if (colorSpan.Length == 6)
        {
            byte r = byte.Parse(colorSpan[0..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte g = byte.Parse(colorSpan[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte b = byte.Parse(colorSpan[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new Color(r, g, b);
        }

        // #RGB 形式の場合 (文字数が3)
        if (colorSpan.Length == 3)
        {
            // 1文字を2回繰り返す (F -> FF)
            byte r = byte.Parse(stackalloc char[] { colorSpan[0], colorSpan[0] }, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
            byte g = byte.Parse(stackalloc char[] { colorSpan[1], colorSpan[1] }, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
            byte b = byte.Parse(stackalloc char[] { colorSpan[2], colorSpan[2] }, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);
            return new Color(r, g, b);
        }

        throw new FormatException($"無効なカラーコード形式です: {webColor}");
    }

    /// <summary>各成分を保持したまま描画用の色へ変換します。</summary>
    /// <param name="color">変換する色値。</param>
    /// <returns>同じ赤、緑、青、不透明度を持つ描画用の色。</returns>
    public static implicit operator SKColor(Color color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    /// <summary>描画用の色から各成分を保持した色値へ変換します。</summary>
    /// <param name="skColor">変換する描画用の色。</param>
    /// <returns>同じ赤、緑、青、不透明度を持つ色値。</returns>
    public static implicit operator Color(SKColor skColor) => new(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
}

/// <summary>色相、彩度、明度で色を表す不変値です。</summary>
/// <param name="Hue">色相を表す0度から360度までの角度。</param>
/// <param name="Saturation">彩度を表す0パーセントから100パーセントまでの値。</param>
/// <param name="Value">明度を表す0パーセントから100パーセントまでの値。</param>
/// <seealso cref="Color"/>
public readonly record struct HSVColor(double Hue, double Saturation, double Value)
{
    /// <summary>0度から360度までの色相を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">設定値が0未満または360を上回っています。</exception>
    public double Hue
    {
        get;
        init
        {
            if (value is < 0 or > 360) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Hue;

    /// <summary>0パーセントから100パーセントまでの彩度を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">設定値が0未満または100を上回っています。</exception>
    public double Saturation
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Saturation;

    /// <summary>0パーセントから100パーセントまでの明度を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">設定値が0未満または100を上回っています。</exception>
    public double Value
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Value;

    /// <summary>赤、緑、青の色値を色相、彩度、明度の表現へ変換します。</summary>
    /// <param name="color">変換する色値。</param>
    /// <returns>変換後の色相、彩度、明度。</returns>
    /// <exception cref="NotImplementedException">この変換は現在実装されていません。</exception>
    public static implicit operator HSVColor(Color color) => throw new NotImplementedException();

    /// <summary>色相、彩度、明度の表現を赤、緑、青の色値へ変換します。</summary>
    /// <param name="skColor">変換する色相、彩度、明度。</param>
    /// <returns>変換後の色値。</returns>
    /// <exception cref="NotImplementedException">この変換は現在実装されていません。</exception>
    public static implicit operator Color(HSVColor skColor) => throw new NotImplementedException();
}

/// <summary>色相、彩度、輝度で色を表す不変値です。</summary>
/// <param name="Hue">色相を表す0度から360度までの角度。</param>
/// <param name="Saturation">彩度を表す0パーセントから100パーセントまでの値。</param>
/// <param name="Value">輝度を表す0パーセントから100パーセントまでの値。</param>
/// <seealso cref="Color"/>
public readonly record struct HSLColor(double Hue, double Saturation, double Value)
{
    /// <summary>0度から360度までの色相を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">設定値が0未満または360を上回っています。</exception>
    public double Hue
    {
        get;
        init
        {
            if (value is < 0 or > 360) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Hue;

    /// <summary>0パーセントから100パーセントまでの彩度を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">設定値が0未満または100を上回っています。</exception>
    public double Saturation
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Saturation;

    /// <summary>0パーセントから100パーセントまでの輝度を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">設定値が0未満または100を上回っています。</exception>
    public double Value
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Value;

    /// <summary>赤、緑、青の色値を色相、彩度、輝度の表現へ変換します。</summary>
    /// <param name="color">変換する色値。</param>
    /// <returns>変換後の色相、彩度、輝度。</returns>
    /// <exception cref="NotImplementedException">この変換は現在実装されていません。</exception>
    public static implicit operator HSLColor(Color color) => throw new NotImplementedException();

    /// <summary>色相、彩度、輝度の表現を赤、緑、青の色値へ変換します。</summary>
    /// <param name="skColor">変換する色相、彩度、輝度。</param>
    /// <returns>変換後の色値。</returns>
    /// <exception cref="NotImplementedException">この変換は現在実装されていません。</exception>
    public static implicit operator Color(HSLColor skColor) => throw new NotImplementedException();
}