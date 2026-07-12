using System.Globalization;
using SkiaSharp;

namespace FloatSoda.Geometrics;

public readonly record struct Color(byte Red = 255, byte Green = 255, byte Blue = 255, byte Alpha = 255)
{
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

    public static implicit operator SKColor(Color color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    public static implicit operator Color(SKColor skColor) => new(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
}

public readonly record struct HSVColor(double Hue, double Saturation, double Value)
{
    public double Hue
    {
        get;
        init
        {
            if (value is < 0 or > 360) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Hue;

    public double Saturation
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Saturation;

    public double Value
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Value;

    public static implicit operator HSVColor(Color color) => throw new NotImplementedException();

    public static implicit operator Color(HSVColor skColor) => throw new NotImplementedException();
}

public readonly record struct HSLColor(double Hue, double Saturation, double Value)
{
    public double Hue
    {
        get;
        init
        {
            if (value is < 0 or > 360) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Hue;

    public double Saturation
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Saturation;

    public double Value
    {
        get;
        init
        {
            if (value is < 0 or > 100) throw new ArgumentOutOfRangeException();
            field = value;
        }
    } = Value;

    public static implicit operator HSLColor(Color color) => throw new NotImplementedException();

    public static implicit operator Color(HSLColor skColor) => throw new NotImplementedException();
}