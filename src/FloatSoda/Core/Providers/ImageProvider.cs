using SkiaSharp;

namespace FloatSoda.Core.Providers;

/// <summary>描画に使用する画像の読み込み方法を表し、共有キャッシュのキーを兼ねます。</summary>
/// <remarks>
/// <para>
/// 画像を使う側は<see cref="ResolveAsync"/>で<see cref="ImageHandle"/>を借り、使い終わったら
/// <see cref="ImageHandle.Dispose"/>で返します。画像そのものはキャッシュが所有し、等しいプロバイダーから
/// 借りたハンドルは同じ画像を共有します。最後のハンドルが返された時点で画像を解放します。
/// </para>
/// <para>
/// 等しいかどうかはrecordの値の等価性で判定します。派生型は、同じ画像を指すインスタンス同士が
/// 等しくなるように、読み込み元を表す値(パスやURLなど)をrecordのメンバーとして持たせてください。
/// </para>
/// </remarks>
/// <seealso cref="FileImageProvider"/>
public abstract record ImageProvider
{
    /// <summary>このプロバイダーが指す画像を借ります。</summary>
    /// <param name="cancellationToken">
    /// この呼び出しの待機をキャンセルするトークン。他の利用者が残っているあいだは、共有している読み込みを中断しません。
    /// この呼び出しが最後の利用者だった場合は、読み込みにもキャンセルを通知します。
    /// </param>
    /// <returns>画像を借りているあいだ保持し、使い終わったら破棄するハンドル。</returns>
    /// <remarks>
    /// 等しいプロバイダーの画像が既に読み込み済みであれば、完了済みの<see cref="ValueTask{TResult}"/>を返します。
    /// 読み込みの失敗は返すタスクの状態として通知し、失敗した結果はキャッシュに残しません。
    /// </remarks>
    public ValueTask<ImageHandle> ResolveAsync(CancellationToken cancellationToken = default) =>
        ImageCache.Shared.ResolveAsync(this, cancellationToken);

    /// <summary>画像を読み込みます。</summary>
    /// <param name="cancellationToken">処理のキャンセルを通知するトークン。</param>
    /// <returns>読み込んだ画像。所有権はキャッシュへ移ります。</returns>
    /// <remarks>
    /// フレームを処理するスレッドから呼び出されます。呼び出しスレッドをブロックせず、
    /// ファイルやネットワークの待ち時間とデコードは非同期に実行してください。
    /// 複数の画像の読み込みは並行して進みます。
    /// </remarks>
    protected abstract ValueTask<SKImage> LoadAsync(CancellationToken cancellationToken);

    internal ValueTask<SKImage> LoadCoreAsync(CancellationToken cancellationToken) => LoadAsync(cancellationToken);
}

/// <summary>ファイルから画像データを読み込むプロバイダーです。</summary>
/// <param name="Path">読み込む画像ファイルのパス。</param>
/// <seealso cref="ImageProvider"/>
public record FileImageProvider(string Path) : ImageProvider
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">画像として解釈できないファイルです。</exception>
    protected override ValueTask<SKImage> LoadAsync(CancellationToken cancellationToken) =>
        new(Task.Run(() => Load(cancellationToken), cancellationToken));

    private SKImage Load(CancellationToken cancellationToken)
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

/// <summary>画像の読み込みの進み具合を表します。</summary>
/// <param name="BytesLoaded">これまでに受け取ったバイト数。</param>
/// <param name="TotalBytes">全体のバイト数。分からない場合は<see langword="null"/>。</param>
/// <remarks>
/// 現在は、プロバイダーが進み具合を報告する手段がありません。ネットワークから読み込むプロバイダーを追加するときに用意します。
/// それまでは、この値を受け取る側へ常に<see langword="null"/>が渡されます。
/// </remarks>
public sealed record ImageLoadingProgress(long BytesLoaded, long? TotalBytes);

/// <summary><see cref="ImageProvider.ResolveAsync"/>で借りた画像を表します。</summary>
/// <remarks>破棄すると画像を返します。破棄したあとは<see cref="Image"/>を使用できません。</remarks>
public sealed class ImageHandle : IDisposable
{
    private readonly SKImage _image;
    private Action? _release;

    internal ImageHandle(SKImage image, Action release)
    {
        _image = image;
        _release = release;
    }

    /// <summary>借りている画像を取得します。この画像を直接破棄しないでください。</summary>
    /// <exception cref="ObjectDisposedException">このハンドルは既に破棄されています。</exception>
    public SKImage Image
    {
        get
        {
            ObjectDisposedException.ThrowIf(_release is null, this);
            return _image;
        }
    }

    /// <summary>借りている画像を返します。複数回呼び出しても安全です。</summary>
    public void Dispose() => Interlocked.Exchange(ref _release, null)?.Invoke();
}
