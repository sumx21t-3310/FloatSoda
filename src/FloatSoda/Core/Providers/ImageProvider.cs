using SkiaSharp;

namespace FloatSoda.Core.Providers;

/// <summary>描画に使用する画像を読み込む方法を表します。</summary>
/// <seealso cref="FileImageProvider"/>
public abstract record ImageProvider : ResourceProvider<SKImage>;

/// <summary>ファイルから画像データを読み込むプロバイダーです。</summary>
/// <param name="Path">読み込む画像ファイルのパス。</param>
/// <seealso cref="ImageProvider"/>
public record FileImageProvider(string Path) : ImageProvider
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">画像として解釈できないファイルです。</exception>
    protected override SKImage Load(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = File.ReadAllBytes(Path);
        cancellationToken.ThrowIfCancellationRequested();

        // FromEncodedDataはデコードできないデータに対してnullを返す。
        // nullのまま返すとRenderImageのレイアウト/描画までnullが流れてNullReferenceExceptionになるため、
        // ここで原因の分かる例外へ変換する。
        return SKImage.FromEncodedData(data)
               ?? throw new InvalidOperationException($"画像として読み込めませんでした: {Path}");
    }
}
