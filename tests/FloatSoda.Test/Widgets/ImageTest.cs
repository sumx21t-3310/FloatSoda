using FloatSoda.Core;
using FloatSoda.Core.Providers;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;
using ImageWidget = FloatSoda.Widgets.Paint.Image;

namespace FloatSoda.Test.Widgets;

public class ImageTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(40, 40);

    /// <summary>指定した単色で塗りつぶしたPNGを一時ファイルへ書き出す。</summary>
    private static string CreateTempPng(SKColor color, int width = 8, int height = 8)
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-image-{Guid.NewGuid():N}.png");
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(color);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
        return path;
    }

    /// <summary>
    /// 画像を先に借りておく。借りているあいだは<see cref="ImageProvider.ResolveAsync"/>が同期的に完了するため、
    /// 1回のビルド・レイアウト・ペイントで画像まで描画される。
    /// </summary>
    private static ImageHandle Preload(string path) =>
        new FileImageProvider(path).ResolveAsync().AsTask().GetAwaiter().GetResult();

    [Fact]
    public void Render_画像を先に借りてからヘッドレスレンダラーで描画_1パスで画像が反映される()
    {
        var path = CreateTempPng(SKColors.Blue);

        try
        {
            using var preloaded = Preload(path);
            var widget = new SizedBox
            {
                Width = Size.Width,
                Height = Size.Height,
                Child = new ImageWidget { Provider = new FileImageProvider(path) }
            };

            using var bitmap = Renderer.Render(widget, Size);

            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 20));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Build_ファイルが存在しない_例外を投げずErrorBuilderの結果を表示する()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-missing-{Guid.NewGuid():N}.png");
        Exception? reported = null;

        var root = MountAndWaitForCompletion(new ImageWidget
        {
            Provider = new FileImageProvider(path),
            ErrorBuilder = (_, exception) =>
            {
                reported = exception;
                return new SizedBox { Width = 12, Height = 12 };
            }
        });

        Assert.IsType<FileNotFoundException>(reported);
        Assert.Equal(new SKSize(12, 12), FindRenderObject<RenderConstrainedBox>(root).Size);
    }

    [Fact]
    public void Build_画像として解釈できないファイル_ErrorBuilderへInvalidOperationExceptionを渡す()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-broken-{Guid.NewGuid():N}.png");
        File.WriteAllText(path, "これはPNGではありません");

        try
        {
            Exception? reported = null;

            MountAndWaitForCompletion(new ImageWidget
            {
                Provider = new FileImageProvider(path),
                ErrorBuilder = (_, exception) =>
                {
                    reported = exception;
                    return new SizedBox();
                }
            });

            Assert.IsType<InvalidOperationException>(reported);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Build_読み込みに失敗しErrorBuilderが未指定_例外を投げず何も表示しない()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-missing-{Guid.NewGuid():N}.png");

        var root = MountAndWaitForCompletion(new ImageWidget { Provider = new FileImageProvider(path) });

        Assert.Equal(SKSize.Empty, FindRenderObject<RenderConstrainedBox>(root).Size);
    }

    [Fact]
    public void Build_読み込みが未完了_LoadingBuilderへnullの進捗を渡して結果を表示する()
    {
        var progressValues = new List<ImageLoadingProgress?>();

        var (root, _) = Mount(new ImageWidget
        {
            Provider = new PendingImageProvider(Guid.NewGuid()),
            LoadingBuilder = (_, progress) =>
            {
                progressValues.Add(progress);
                return new SizedBox { Width = 7, Height = 7 };
            }
        });

        Assert.Equal([null], progressValues);
        Assert.Equal(new SKSize(7, 7), FindRenderObject<RenderConstrainedBox>(root).Size);
    }

    [Fact]
    public void Build_読み込みが完了_LoadingBuilderの結果を画像へ置き換える()
    {
        var path = CreateTempPng(SKColors.Blue);

        try
        {
            var root = MountAndWaitForCompletion(new ImageWidget
            {
                Provider = new FileImageProvider(path),
                LoadingBuilder = (_, _) => new SizedBox { Width = 7, Height = 7 }
            });

            Assert.Equal(new SKSize(8, 8), FindRenderObject<RenderImage>(root).Size);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DidUpdateWidget_読み込み済みのProviderを未完了のProviderへ差し替え_返却済みの画像を使わずLoadingBuilderへ戻る()
    {
        // 回帰テスト: TaskBuilderは差し替え直後も前回のDataを保持する。
        // 返却済みのハンドルから画像を取り出すと、Build中にObjectDisposedExceptionでアプリ全体が停止する。
        var path = CreateTempPng(SKColors.Blue);

        try
        {
            using var preloaded = Preload(path);
            var (root, owner) = Mount(new ImageWidget { Provider = new FileImageProvider(path) });
            Assert.NotNull(FindRenderObjectOrDefault<RenderImage>(root));

            Update(root, owner, new ImageWidget
            {
                Provider = new PendingImageProvider(Guid.NewGuid()),
                LoadingBuilder = (_, _) => new SizedBox { Width = 7, Height = 7 }
            });

            Assert.Null(FindRenderObjectOrDefault<RenderImage>(root));
            Assert.Equal(new SKSize(7, 7), FindRenderObject<RenderConstrainedBox>(root).Size);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Dispose_ツリーから外す_借りていた画像を返す()
    {
        var path = CreateTempPng(SKColors.Blue);

        try
        {
            SKImage image;
            using (var preloaded = Preload(path))
            {
                image = preloaded.Image;
                var (root, owner) = Mount(new ImageWidget { Provider = new FileImageProvider(path) });

                Update(root, owner, new SizedBox());
            }

            // テスト側のハンドルとImage側のハンドルの両方が返されたので、画像は解放されている。
            Assert.Equal(IntPtr.Zero, image.Handle);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Dispose_読み込みが未完了のままツリーから外す_読み込みを中断して参照を返す()
    {
        // 回帰テスト: 待機をキャンセルしないと、完了しない読み込みの参照がStateの破棄後も残り続ける。
        var recorder = new TokenRecorder();
        var (root, owner) = Mount(new ImageWidget { Provider = new PendingImageProvider(Guid.NewGuid(), recorder) });
        Assert.False(recorder.Token.IsCancellationRequested);

        Update(root, owner, new SizedBox());

        // 待機のキャンセルから参照の返却までは、スレッドプール上の継続で進む。
        Assert.True(SpinWait.SpinUntil(() => recorder.Token.IsCancellationRequested, TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public void DidUpdateWidget_読み込みが未完了のままProviderを差し替え_前の読み込みを中断する()
    {
        var recorder = new TokenRecorder();
        var (root, owner) = Mount(new ImageWidget { Provider = new PendingImageProvider(Guid.NewGuid(), recorder) });

        Update(root, owner, new ImageWidget { Provider = new PendingImageProvider(Guid.NewGuid()) });

        // 待機のキャンセルから参照の返却までは、スレッドプール上の継続で進む。
        Assert.True(SpinWait.SpinUntil(() => recorder.Token.IsCancellationRequested, TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public void Render_ヘッドレスレンダラーで描画した後_Imageが借りた画像を返している()
    {
        // 回帰テスト: 描画後にWidgetツリーを外さないと、Imageが借りたハンドルが返らず画像が解放されない。
        var path = CreateTempPng(SKColors.Blue);

        try
        {
            SKImage image;
            using (var preloaded = Preload(path))
            {
                image = preloaded.Image;
                using var bitmap = Renderer.Render(new ImageWidget { Provider = new FileImageProvider(path) }, Size);
            }

            Assert.Equal(IntPtr.Zero, image.Handle);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void PerformLayout_緩い制約_画像の原寸を自身のサイズにする()
    {
        using var image = CreateSolidImage();
        var renderImage = new RenderImage { Image = image };

        renderImage.Layout(new BoxConstraints(0, 100, 0, 100));

        Assert.Equal(new SKSize(8, 8), renderImage.Size);
    }

    [Fact]
    public void Fit_既定は横長画像の縦横比を維持し上下に余白ができる()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 4);

        try
        {
            using var preloaded = Preload(path);
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path) }
                },
                Size);

            // 8x4の画像を40x40へContainで収めると40x20になり、上下へ10pxずつ余白ができる。
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 20));
            Assert.Equal(default, bitmap.GetPixel(20, 2));
            Assert.Equal(default, bitmap.GetPixel(20, 37));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_Fill_領域全体を埋める()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 4);

        try
        {
            using var preloaded = Preload(path);
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path), Fit = BoxFit.Fill }
                },
                Size);

            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 2));
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 37));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_None_画像を拡大せず原寸で中央へ配置する()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 8);

        try
        {
            using var preloaded = Preload(path);
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path), Fit = BoxFit.None }
                },
                Size);

            // 8x8が中央(16,16)-(24,24)へ置かれ、その外側は描画されない。
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 20));
            Assert.Equal(default, bitmap.GetPixel(2, 2));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Alignment_TopLeft_収めた画像を上端へ寄せる()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 4);

        try
        {
            using var preloaded = Preload(path);
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget
                    {
                        Provider = new FileImageProvider(path),
                        Alignment = Alignment.TopLeft
                    }
                },
                Size);

            // Containで40x20になった画像が上端へ寄るため、上が塗られ下が余白になる。
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 2));
            Assert.Equal(default, bitmap.GetPixel(20, 37));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_Cover_描画元を切り取って領域全体を埋める()
    {
        // 左半分が赤、右半分が青の16x4を40x40へCoverで収める。
        // Sourceは中央の4x4へ切り取られるので、赤と青の境目が領域の中央に来る。
        // 切り取りではなく引き伸ばしなら、境目は中央に来ない。
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-image-{Guid.NewGuid():N}.png");
        using (var bitmap = new SKBitmap(16, 4))
        {
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    bitmap.SetPixel(x, y, x < 8 ? SKColors.Red : SKColors.Blue);
                }
            }

            using var encoded = SKImage.FromBitmap(bitmap);
            using var data = encoded.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.Create(path);
            data.SaveTo(stream);
        }

        try
        {
            using var preloaded = Preload(path);
            using var rendered = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path), Fit = BoxFit.Cover }
                },
                Size);

            // 領域全体が埋まる(余白が残らない)。
            Assert.Equal(SKColors.Red, rendered.GetPixel(2, 2));
            Assert.Equal(SKColors.Blue, rendered.GetPixel(37, 37));
            // 切り取り後の境目が中央に来る。
            Assert.Equal(SKColors.Red, rendered.GetPixel(18, 20));
            Assert.Equal(SKColors.Blue, rendered.GetPixel(22, 20));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_定義されていない値_ArgumentOutOfRangeExceptionを投げる()
    {
        using var image = CreateSolidImage();

        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderImage { Image = image, Fit = (BoxFit)99 });
    }

    [Fact]
    public void Alignment_成分が有限値でない_ArgumentOutOfRangeExceptionを投げる()
    {
        using var image = CreateSolidImage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RenderImage { Image = image, Alignment = new Alignment(float.NaN, 0) });
    }

    private static SKImage CreateSolidImage()
    {
        using var bitmap = new SKBitmap(8, 8);
        bitmap.Erase(SKColors.Blue);
        return SKImage.FromBitmap(bitmap);
    }

    /// <summary>LoadAsyncへ渡されたトークンを記録する。recordの等価性を変えないよう、プロバイダーの外に持つ。</summary>
    private sealed class TokenRecorder
    {
        public CancellationToken Token { get; set; }
    }

    /// <summary>読み込みが完了しないプロバイダー。</summary>
    private sealed record PendingImageProvider(Guid Key, TokenRecorder? Recorder = null) : ImageProvider
    {
        private readonly TaskCompletionSource<SKImage> _source = new();

        protected override ValueTask<SKImage> LoadAsync(CancellationToken cancellationToken)
        {
            if (Recorder is not null) Recorder.Token = cancellationToken;
            return new ValueTask<SKImage>(_source.Task);
        }
    }

    private static (RenderObjectToWidgetElement<RenderView> Root, BuildOwner Owner) Mount(
        Widget widget,
        Action? onBuildScheduled = null)
    {
        var renderView = new RenderView(100, 100);
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        var owner = new BuildOwner(onBuildScheduled ?? (() => { }));
        var root = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = new Align { Child = widget }
        }.AttachToRenderTree(owner, null);

        renderView.PrepareInitialFrame();
        pipeline.FlushLayout();
        return (root, owner);
    }

    /// <summary>マウントしたあと、読み込みの完了がBuildOwnerへ戻ってくるのを待って再ビルドする。</summary>
    private static RenderObjectToWidgetElement<RenderView> MountAndWaitForCompletion(Widget widget)
    {
        using var buildScheduled = new ManualResetEventSlim();
        var (root, owner) = Mount(widget, buildScheduled.Set);

        Assert.True(buildScheduled.Wait(TimeSpan.FromSeconds(5)), "読み込みの完了が通知されませんでした。");
        owner.BuildScope();
        root.RenderObject!.Owner!.FlushLayout();
        return root;
    }

    private static void Update(RenderObjectToWidgetElement<RenderView> root, BuildOwner owner, Widget widget)
    {
        new RenderObjectToWidgetAdapter
        {
            Container = (RenderView)root.RenderObject!,
            Child = new Align { Child = widget }
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();
        root.RenderObject!.Owner!.FlushLayout();
    }

    private static T FindRenderObject<T>(RenderObjectToWidgetElement<RenderView> root) where T : RenderObject =>
        FindRenderObjectOrDefault<T>(root) ?? throw new InvalidOperationException($"{typeof(T).Name} が見つかりません。");

    private static T? FindRenderObjectOrDefault<T>(RenderObjectToWidgetElement<RenderView> root) where T : RenderObject
    {
        T? found = null;
        Visit(root.RenderObject!);
        return found;

        void Visit(RenderObject node)
        {
            if (found is not null) return;
            if (node is T match)
            {
                found = match;
                return;
            }

            node.VisitChildren(Visit);
        }
    }
}
