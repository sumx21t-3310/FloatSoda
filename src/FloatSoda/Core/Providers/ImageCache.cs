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
                entry = new Entry(provider, Evict);
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

    /// <summary>
    /// 失敗またはキャンセルされた読み込みを、待機者が参照を返すのを待たずに取り除く。
    /// 残しておくと、待機者がまだ残っているあいだの再試行が失敗済みの結果へ相乗りし、読み込み直さない。
    /// </summary>
    private void Evict(Entry entry)
    {
        lock (_gate) RemoveIfCurrent(entry);
    }

    // 取り除かれたあとに同じプロバイダーで新しいエントリが作られている場合がある。自分のエントリだけを取り除く。
    private void RemoveIfCurrent(Entry entry)
    {
        if (_entries.TryGetValue(entry.Provider, out var current) && ReferenceEquals(current, entry))
        {
            _entries.Remove(entry.Provider);
        }
    }

    private void Release(Entry entry)
    {
        lock (_gate)
        {
            if (--entry.ReferenceCount > 0) return;

            RemoveIfCurrent(entry);
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
        internal Entry(ImageProvider provider, Action<Entry> onFailed)
        {
            Provider = provider;
            Load = new Lazy<Task<SKImage>>(
                () =>
                {
                    var load = StartAsync(provider, Cancellation.Token);

                    // 待機者の継続より先に登録する。継続は登録順に走るため、
                    // 待機者が失敗を観測した時点で、このエントリは既に取り除かれている。
                    _ = load.ContinueWith(
                        _ => onFailed(this),
                        CancellationToken.None,
                        TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default);
                    return load;
                },
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
