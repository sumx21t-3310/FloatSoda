using SkiaSharp;

namespace FloatSoda.Geometrics;

/// <summary>入力ボックスを出力ボックスへ収める方法を指定します。</summary>
/// <remarks><see cref="FloatSoda.Widgets.Layout.FittedBox"/>と<see cref="FloatSoda.Widgets.Paint.Image"/>で共通して使用します。</remarks>
public enum BoxFit
{
    /// <summary>入力の縦横比を変えて出力ボックス全体を埋めます。</summary>
    Fill,

    /// <summary>入力全体が収まる範囲で縦横比を維持して最大化します。</summary>
    Contain,

    /// <summary>出力全体を覆う範囲で縦横比を維持して最小化します。</summary>
    Cover,

    /// <summary>入力の幅全体が表示されるように縦横比を維持して拡大縮小します。</summary>
    FitWidth,

    /// <summary>入力の高さ全体が表示されるように縦横比を維持して拡大縮小します。</summary>
    FitHeight,

    /// <summary>入力を拡大縮小せずに配置し、出力に収まらない部分を除外します。</summary>
    None,

    /// <summary>入力が出力より大きい場合だけ、全体が収まるように縮小します。</summary>
    ScaleDown,
}

/// <summary><see cref="BoxFit"/>を適用した結果の、入力側と出力側の矩形サイズです。</summary>
/// <param name="Source">入力のうち描画に使用する部分の大きさ。入力全体より小さい場合、残りは切り取られます。</param>
/// <param name="Destination">出力上に描画される矩形の大きさ。出力より大きい場合、はみ出した部分の扱いは呼び出し側が決めます。</param>
internal readonly record struct FittedSizes(SKSize Source, SKSize Destination);

/// <summary><see cref="BoxFit"/>を具体的な矩形サイズへ適用します。</summary>
internal static class BoxFitExtensions
{
    /// <summary>入力サイズを出力サイズへ収めたときの、入力側と出力側の矩形サイズを算出します。</summary>
    /// <param name="fit">収め方。</param>
    /// <param name="input">収める対象の大きさ。</param>
    /// <param name="output">収め先の大きさ。</param>
    /// <returns>算出した入力側と出力側の矩形サイズ。いずれかが空の場合は両方が空になります。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fit"/>が定義されていない値です。</exception>
    internal static FittedSizes Apply(this BoxFit fit, SKSize input, SKSize output)
    {
        if (input.IsEmpty || output.IsEmpty) return new FittedSizes(SKSize.Empty, SKSize.Empty);

        var inputAspectRatio = input.Width / input.Height;
        var outputAspectRatio = output.Width / output.Height;

        return fit switch
        {
            BoxFit.Fill => new FittedSizes(input, output),
            BoxFit.Contain => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(input, new SKSize(input.Width * output.Height / input.Height, output.Height))
                : new FittedSizes(input, new SKSize(output.Width, input.Height * output.Width / input.Width)),
            BoxFit.Cover => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(new SKSize(input.Width, input.Width * output.Height / output.Width), output)
                : new FittedSizes(new SKSize(input.Height * output.Width / output.Height, input.Height), output),
            BoxFit.FitWidth => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(new SKSize(input.Width, input.Width * output.Height / output.Width), output)
                : new FittedSizes(input, new SKSize(output.Width, input.Height * output.Width / input.Width)),
            BoxFit.FitHeight => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(input, new SKSize(input.Width * output.Height / input.Height, output.Height))
                : new FittedSizes(new SKSize(input.Height * output.Width / output.Height, input.Height), output),
            BoxFit.None => new FittedSizes(
                new SKSize(Math.Min(input.Width, output.Width), Math.Min(input.Height, output.Height)),
                new SKSize(Math.Min(input.Width, output.Width), Math.Min(input.Height, output.Height))),
            BoxFit.ScaleDown => ScaleDown(input, output),
            _ => throw new ArgumentOutOfRangeException(nameof(fit), fit, "定義済みのBoxFitを指定してください。"),
        };
    }

    /// <summary>指定した値が<see cref="BoxFit"/>として定義済みであることを検証します。</summary>
    /// <param name="value">検証する値。</param>
    /// <param name="parameterName">例外に含める引数名。</param>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    internal static void Validate(BoxFit value, string parameterName)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "定義済みのBoxFitを指定してください。");
        }
    }

    private static FittedSizes ScaleDown(SKSize input, SKSize output)
    {
        var destination = input;
        var aspectRatio = input.Width / input.Height;
        if (destination.Height > output.Height)
        {
            destination = new SKSize(output.Height * aspectRatio, output.Height);
        }

        if (destination.Width > output.Width)
        {
            destination = new SKSize(output.Width, output.Width / aspectRatio);
        }

        return new FittedSizes(input, destination);
    }
}
