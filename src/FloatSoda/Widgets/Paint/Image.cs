using FloatSoda.Core.Providers;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// 画像プロバイダーから読み込んだ画像を自身の領域へ描画します。
/// </summary>
/// <remarks>
/// 読み込みが完了するまでは<see cref="LoadingBuilder"/>、読み込みに失敗した場合は<see cref="ErrorBuilder"/>が
/// 返すウィジェットを代わりに表示します。指定しない場合は何も表示しません。
/// 失敗してもアプリケーションは停止しません。
/// 等しい<see cref="Provider"/>を使う<see cref="Image"/>同士は、読み込んだ画像を共有します。
/// </remarks>
/// <seealso cref="RenderImage"/>
public record Image : StatefulWidget<Image>
{
    /// <summary>
    /// 描画する画像を読み込むプロバイダーを取得します。
    /// </summary>
    public required ImageProvider Provider { get; init; }

    /// <summary>画像を自身の領域へ収める方法を取得します。</summary>
    /// <remarks>
    /// 既定は<see cref="BoxFit.Contain"/>で、縦横比を維持したまま領域内へ収めます。
    /// <see cref="BoxFit.Cover"/>のように画像の一部だけを使う場合は描画元の矩形を切り取るため、
    /// どの値でも領域外へはみ出しません。
    /// </remarks>
    public BoxFit Fit { get; init; } = BoxFit.Contain;

    /// <summary>収めた画像を自身の領域内へ配置する位置を取得します。</summary>
    public Alignment Alignment { get; init; } = Alignment.Center;

    /// <summary>読み込みが完了するまで、画像の代わりに表示するウィジェットを構築する処理を取得します。</summary>
    /// <remarks>
    /// 第2引数は読み込みの進み具合です。進み具合を報告しないプロバイダー
    /// (<see cref="FileImageProvider"/>など)では常に<see langword="null"/>です。
    /// </remarks>
    public Func<IBuildContext, ImageLoadingProgress?, Widget>? LoadingBuilder { get; init; }

    /// <summary>読み込みに失敗したとき、画像の代わりに表示するウィジェットを構築する処理を取得します。</summary>
    /// <remarks>第2引数は失敗の原因です。</remarks>
    public Func<IBuildContext, Exception, Widget>? ErrorBuilder { get; init; }

    /// <inheritdoc />
    public override State<Image> CreateState() => new ImageState();

    private sealed class ImageState : State<Image>
    {
        private Task<ImageHandle> _loadTask = null!;

        public override void InitState() => StartLoading();

        public override void DidUpdateWidget(Image oldWidget)
        {
            if (oldWidget.Provider != Widget!.Provider)
            {
                StartLoading();
            }
        }

        public override Widget Build(IBuildContext context) => new TaskBuilder<ImageHandle>
        {
            Task = _loadTask,
            Builder = BuildSnapshot
        };

        /// <summary>
        /// このStateがツリーから外れるときに、借りていた画像を返す。
        /// 返さないとキャッシュが画像(ネイティブメモリ)を保持し続ける。
        /// </summary>
        public override void Dispose()
        {
            DisposeLoadedImage(_loadTask);
            base.Dispose();
        }

        private void StartLoading()
        {
            // Providerが差し替わった場合、前回借りた画像はこのStateではもう使わないためここで返す。
            DisposeLoadedImage(_loadTask);
            _loadTask = Widget!.Provider.ResolveAsync().AsTask();
        }

        /// <summary>
        /// 借り終わっていればハンドルを破棄する。未完了のタスクは完了後に破棄されるよう継続を登録する。
        /// </summary>
        private static void DisposeLoadedImage(Task<ImageHandle>? task)
        {
            if (task is null) return;

            if (!task.IsCompleted)
            {
                // OnlyOnRanToCompletionにすると失敗時に継続が走らない。Stateが先に破棄されていると
                // BuildSnapshotも呼ばれないため、例外が未観測のまま残りUnobservedTaskExceptionになる。
                // 失敗も含めて完了時に必ず走らせ、例外を観測する。
                _ = task.ContinueWith(
                    static completed =>
                    {
                        if (completed.IsCompletedSuccessfully) completed.Result.Dispose();
                        else _ = completed.Exception;
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
                return;
            }

            if (task.IsCompletedSuccessfully) task.Result.Dispose();
            else _ = task.Exception;
        }

        private Widget BuildSnapshot(IBuildContext context, TaskSnapshot<ImageHandle> snapshot)
        {
            // Providerを差し替えた直後のSnapshotは、前回のData(返却済みのハンドル)を持ったままWaitingになる。
            // 完了したタスクの結果だけを画像として扱う。
            if (snapshot is { ConnectionState: TaskConnectionState.Done, HasData: true })
            {
                return new ResolvedImage
                {
                    Image = snapshot.Data!.Image,
                    Fit = Widget!.Fit,
                    Alignment = Widget!.Alignment
                };
            }

            // 読み込み失敗をここで再スローするとBuildScope→DrawFrameを貫通し、
            // FloatSodaApp.MainLoopのcatchでアプリ全体が停止してしまう。
            // 1枚の画像の失敗を全画面消失に広げないため、ErrorBuilder(またはプレースホルダー)へフォールバックする。
            if (snapshot.HasError)
            {
                return Widget!.ErrorBuilder?.Invoke(context, snapshot.Error!) ?? new SizedBox();
            }

            return Widget!.LoadingBuilder?.Invoke(context, null) ?? new SizedBox();
        }
    }

    private sealed record ResolvedImage : LeafRenderObjectWidget<RenderImage>
    {
        public required SkiaSharp.SKImage Image { get; init; }

        public required BoxFit Fit { get; init; }

        public required Alignment Alignment { get; init; }

        public override RenderImage CreateRenderObject() =>
            new() { Image = Image, Fit = Fit, Alignment = Alignment };

        public override void UpdateRenderObject(RenderImage renderObject)
        {
            renderObject.Image = Image;
            renderObject.Fit = Fit;
            renderObject.Alignment = Alignment;
        }
    }
}
