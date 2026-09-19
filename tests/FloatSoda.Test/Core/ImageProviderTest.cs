using FloatSoda.Core.Providers;
using SkiaSharp;

namespace FloatSoda.Test.Core;

public class ImageProviderTest
{
    /// <summary>読み込みの開始回数と、完了のタイミングをテストから制御するための共有状態。</summary>
    private sealed class LoadControl
    {
        private int _loadCount;

        public int LoadCount => _loadCount;

        public TaskCompletionSource<SKImage> Source { get; private set; } = NewSource();

        public CancellationToken LastToken { get; private set; }

        public ValueTask<SKImage> Begin(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _loadCount);
            LastToken = cancellationToken;
            return new ValueTask<SKImage>(Source.Task);
        }

        /// <summary>次の読み込みが新しいタスクを待つようにする。</summary>
        public void Reset() => Source = NewSource();

        private static TaskCompletionSource<SKImage> NewSource() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    /// <summary>Keyが等しく同じLoadControlを共有するインスタンス同士が、recordの等価性で等しくなる。</summary>
    private sealed record ControlledImageProvider(Guid Key, LoadControl Control) : ImageProvider
    {
        protected override ValueTask<SKImage> LoadAsync(CancellationToken cancellationToken) =>
            Control.Begin(cancellationToken);
    }

    private sealed record ThrowingImageProvider(Guid Key) : ImageProvider
    {
        protected override ValueTask<SKImage> LoadAsync(CancellationToken cancellationToken) =>
            throw new InvalidOperationException("同期的な失敗");
    }

    private sealed record NullImageProvider(Guid Key) : ImageProvider
    {
        protected override ValueTask<SKImage> LoadAsync(CancellationToken cancellationToken) => new((SKImage)null!);
    }

    private static SKImage CreateImage()
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Blue);
        return SKImage.FromBitmap(bitmap);
    }

    [Fact]
    public async Task ResolveAsync_等しいプロバイダーを同時に解決_読み込みは1回で同じ画像を共有する()
    {
        var control = new LoadControl();
        var key = Guid.NewGuid();
        var cache = new ImageCache();

        var first = cache.ResolveAsync(new ControlledImageProvider(key, control), CancellationToken.None);
        var second = cache.ResolveAsync(new ControlledImageProvider(key, control), CancellationToken.None);
        control.Source.SetResult(CreateImage());

        using var firstHandle = await first;
        using var secondHandle = await second;

        Assert.Equal(1, control.LoadCount);
        Assert.Same(firstHandle.Image, secondHandle.Image);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public async Task ResolveAsync_読み込み済みの画像を借りている_同期的に完了する()
    {
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        control.Source.SetResult(CreateImage());
        using var held = await cache.ResolveAsync(provider, CancellationToken.None);

        var resolve = cache.ResolveAsync(provider, CancellationToken.None);

        Assert.True(resolve.IsCompletedSuccessfully);
        resolve.Result.Dispose();
    }

    [Fact]
    public async Task Dispose_借りているハンドルが残っている_画像を解放しない()
    {
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        var image = CreateImage();
        control.Source.SetResult(image);
        var first = await cache.ResolveAsync(provider, CancellationToken.None);
        using var second = await cache.ResolveAsync(provider, CancellationToken.None);

        first.Dispose();

        Assert.NotEqual(IntPtr.Zero, image.Handle);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public async Task Dispose_最後のハンドル_画像を解放し次回は読み込み直す()
    {
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        var image = CreateImage();
        control.Source.SetResult(image);
        var handle = await cache.ResolveAsync(provider, CancellationToken.None);

        handle.Dispose();

        Assert.Equal(IntPtr.Zero, image.Handle);
        Assert.Equal(0, cache.Count);

        control.Reset();
        control.Source.SetResult(CreateImage());
        using var reloaded = await cache.ResolveAsync(provider, CancellationToken.None);
        Assert.Equal(2, control.LoadCount);
    }

    [Fact]
    public async Task Dispose_複数回呼び出す_参照を1回だけ返す()
    {
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        var image = CreateImage();
        control.Source.SetResult(image);
        var first = await cache.ResolveAsync(provider, CancellationToken.None);
        using var second = await cache.ResolveAsync(provider, CancellationToken.None);

        first.Dispose();
        first.Dispose();

        Assert.NotEqual(IntPtr.Zero, image.Handle);
    }

    [Fact]
    public async Task Image_破棄後に取得_ObjectDisposedExceptionを投げる()
    {
        var control = new LoadControl();
        control.Source.SetResult(CreateImage());
        var handle = await new ImageCache().ResolveAsync(
            new ControlledImageProvider(Guid.NewGuid(), control), CancellationToken.None);

        handle.Dispose();

        Assert.Throws<ObjectDisposedException>(() => handle.Image);
    }

    [Fact]
    public async Task ResolveAsync_読み込みに失敗_失敗をキャッシュせず次回は読み込み直す()
    {
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        control.Source.SetException(new IOException("失敗"));

        await Assert.ThrowsAsync<IOException>(async () => await cache.ResolveAsync(provider, CancellationToken.None));
        Assert.Equal(0, cache.Count);

        control.Reset();
        control.Source.SetResult(CreateImage());
        using var handle = await cache.ResolveAsync(provider, CancellationToken.None);
        Assert.Equal(2, control.LoadCount);
    }

    [Fact]
    public async Task ResolveAsync_失敗を観測した直後に再試行_ほかの待機者の解放を待たずに読み込み直す()
    {
        // 回帰テスト: 失敗済みのエントリを待機者の参照が0になるまで残すと、
        // ほかの待機者がまだ参照を返していないあいだの再試行が、失敗済みの結果へ相乗りする。
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        var first = cache.ResolveAsync(provider, CancellationToken.None);
        var second = cache.ResolveAsync(provider, CancellationToken.None);
        control.Source.SetException(new IOException("失敗"));
        await Assert.ThrowsAsync<IOException>(async () => await first);

        control.Reset();
        control.Source.SetResult(CreateImage());
        using var handle = await cache.ResolveAsync(provider, CancellationToken.None);

        Assert.Equal(2, control.LoadCount);
        await Assert.ThrowsAsync<IOException>(async () => await second);
        // 遅れて参照を返した古いエントリが、新しいエントリを取り除いていない。
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public async Task ResolveAsync_LoadAsyncが同期的に例外を投げる_返すタスクの失敗として通知する()
    {
        var resolve = new ImageCache().ResolveAsync(new ThrowingImageProvider(Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await resolve);
    }

    [Fact]
    public async Task ResolveAsync_LoadAsyncがnullを返す_InvalidOperationExceptionを投げる()
    {
        var resolve = new ImageCache().ResolveAsync(new NullImageProvider(Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await resolve);
    }

    [Fact]
    public void ResolveAsync_異なるプロバイダー_前の読み込みの完了を待たずに次の読み込みを始める()
    {
        var firstControl = new LoadControl();
        var secondControl = new LoadControl();
        var cache = new ImageCache();

        _ = cache.ResolveAsync(new ControlledImageProvider(Guid.NewGuid(), firstControl), CancellationToken.None);
        _ = cache.ResolveAsync(new ControlledImageProvider(Guid.NewGuid(), secondControl), CancellationToken.None);

        // どちらも未完了のまま、2件とも読み込みが始まっている。
        Assert.False(firstControl.Source.Task.IsCompleted);
        Assert.Equal(1, firstControl.LoadCount);
        Assert.Equal(1, secondControl.LoadCount);

        firstControl.Source.SetCanceled();
        secondControl.Source.SetCanceled();
    }

    [Fact]
    public async Task ResolveAsync_待機をキャンセル_参照を返し読み込みを中断する()
    {
        var control = new LoadControl();
        var cache = new ImageCache();
        using var cancellation = new CancellationTokenSource();
        var resolve = cache.ResolveAsync(new ControlledImageProvider(Guid.NewGuid(), control), cancellation.Token);

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await resolve);
        Assert.Equal(0, cache.Count);
        Assert.True(control.LastToken.IsCancellationRequested);
        control.Source.SetCanceled();
    }

    [Fact]
    public async Task ResolveAsync_待機をキャンセルしても他の利用者が残っている_読み込みを中断しない()
    {
        var control = new LoadControl();
        var provider = new ControlledImageProvider(Guid.NewGuid(), control);
        var cache = new ImageCache();
        using var cancellation = new CancellationTokenSource();
        var canceled = cache.ResolveAsync(provider, cancellation.Token);
        var remaining = cache.ResolveAsync(provider, CancellationToken.None);

        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await canceled);

        Assert.False(control.LastToken.IsCancellationRequested);
        control.Source.SetResult(CreateImage());
        using var handle = await remaining;
        Assert.NotNull(handle.Image);
    }

    [Fact]
    public async Task ResolveAsync_利用者がいなくなったあとで読み込みが完了_画像を解放する()
    {
        var control = new LoadControl();
        var cache = new ImageCache();
        using var cancellation = new CancellationTokenSource();
        var resolve = cache.ResolveAsync(new ControlledImageProvider(Guid.NewGuid(), control), cancellation.Token);
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await resolve);
        var image = CreateImage();

        // キャンセルを無視して完了するプロバイダーでも、画像が取り残されない。
        control.Source.SetResult(image);

        await WaitUntilAsync(() => image.Handle == IntPtr.Zero);
        Assert.Equal(IntPtr.Zero, image.Handle);
    }

    [Fact]
    public async Task FileImageProvider_パスが等しい_等しいプロバイダーとして画像を共有する()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-provider-{Guid.NewGuid():N}.png");
        using (var image = CreateImage())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        await using (var stream = File.Create(path))
        {
            data.SaveTo(stream);
        }

        try
        {
            using var first = await new FileImageProvider(path).ResolveAsync();
            using var second = await new FileImageProvider(path).ResolveAsync();

            Assert.Same(first.Image, second.Image);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }
    }
}
