using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace FloatSoda.Core;

/// <summary>描画に使用する画像を読み込む方法を表します。</summary>
/// <seealso cref="FileImageProvider"/>
public abstract record ImageProvider
{
    /// <summary>画像データを読み込みます。</summary>
    /// <returns>呼び出し元が破棄する画像オブジェクト。</returns>
    public abstract SKImage Load();
}

/// <summary>ファイルから画像データを読み込むプロバイダーです。</summary>
/// <param name="Path">読み込む画像ファイルのパス。</param>
/// <seealso cref="ImageProvider"/>
public record FileImageProvider(string Path) : ImageProvider
{
    /// <inheritdoc />
    public override SKImage Load()
    {
        IHost host;
        
        using var stream = File.OpenRead(Path);
        return SKImage.FromEncodedData(stream);
    }
}