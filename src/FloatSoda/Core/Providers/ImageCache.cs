using SkiaSharp;

namespace FloatSoda.Core.Providers;

/// <summary>
/// 読み込んだ画像を所有し、等しい<see cref="ImageProvider"/>の利用者へ同じ画像を貸し出します。
/// </summary>
/// <remarks>
/// 画像を保持するのは借りている<see cref="ImageHandle"/>が1つ以上あるあいだだけです。
/// 最後のハンドルが返されると、画像を解放してエントリを取り除きます。
/// </remarks>
internal sealed class ImageCache
{
    internal static ImageCache Shared { get; } = new();

    private readonly Lock _gate = new();
    private readonly Dictionary<ImageProvider, Entry> _entries = [];

    /// <summary>借りられている画像の数を取得します。</summary>
    internal int Count
    {
        get
        {
            lock (_gate) return _entries.Count;
        }
    }

    internal ValueTask<ImageHandle> ResolveAsync(ImageProvider provider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(provider);

        Entry? entry;
        lock (_gate)
        {
            if (!_entries.TryGetValue(provider, out entry))
            {
                entry = new Entry(provider);
                _entries.Add(provider, entry);
            }

            entry.ReferenceCount++;
        }

        // 読み込みの開始はロックの外で行う。LoadAsyncは利用者のコードなので、ロックを持ったまま呼び出さない。
        var load = entry.Load.Value;

        // 読み込み済みなら同期的に完了させる。1回のビルドで画像まで反映する経路(事前に借りておいた場合)を成立させる。
        return load.IsCompletedSuccessfully
            ? new ValueTask<ImageHandle>(CreateHandle(entry, load.Result))
            : new ValueTask<ImageHandle>(WaitAsync(entry, load, cancellationToken));
    }

    private async Task<ImageHandle> WaitAsync(Entry entry, Task<SKImage> load, CancellationToken cancellationToken)
    {
        try
        {
            var image = await load.WaitAsync(cancellationToken).ConfigureAwait(false);
            return CreateHandle(entry, image);
        }
        catch
        {
            Release(entry);
            throw;
        }
    }

    private ImageHandle CreateHandle(Entry entry, SKImage image) => new(image, () => Release(entry));

    private void Release(Entry entry)
    {
        lock (_gate)
        {
            if (--entry.ReferenceCount > 0) return;

            _entries.Remove(entry.Provider);
        }

        // 誰も待っていない読み込みは中断する。完了していれば画像を解放し、失敗していれば例外を観測する。
        entry.Cancellation.Cancel();
        _ = entry.Load.Value.ContinueWith(
            completed =>
            {
                if (completed.IsCompletedSuccessfully) completed.Result.Dispose();
                else _ = completed.Exception;

                entry.Cancellation.Dispose();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private sealed class Entry
    {
        internal Entry(ImageProvider provider)
        {
            Provider = provider;
            Load = new Lazy<Task<SKImage>>(
                () => StartAsync(provider, Cancellation.Token),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        internal ImageProvider Provider { get; }

        internal CancellationTokenSource Cancellation { get; } = new();

        internal Lazy<Task<SKImage>> Load { get; }

        internal int ReferenceCount { get; set; }

        // asyncメソッドにすることで、LoadAsyncが同期的に投げた例外もタスクの失敗として扱う。
        private static async Task<SKImage> StartAsync(ImageProvider provider, CancellationToken cancellationToken) =>
            await provider.LoadCoreAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{provider.GetType().Name}.LoadAsync が null を返しました。");
    }
}
