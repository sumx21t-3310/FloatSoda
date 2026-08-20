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
/// 読み込みが完了するまで、および読み込みに失敗した場合は<see cref="Child"/>だけを描画します。
/// 失敗はアプリケーションを停止させず、<see cref="OnError"/>で通知します。
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

    /// <summary>画像の上に配置する子ウィジェットを取得します。</summary>
    public Widget? Child { get; init; }

    /// <summary>画像の読み込みに失敗したときに一度だけ呼び出す処理を取得します。</summary>
    /// <remarks>指定しない場合、失敗は通知されずプレースホルダー表示のみになります。</remarks>
    public Action<Exception>? OnError { get; init; }

    /// <inheritdoc />
    public override State<Image> CreateState() => new ImageState();

    private sealed class ImageState : State<Image>
    {
        private Task<SkiaSharp.SKImage> _loadTask = null!;
        private bool _reportedError;

        public override void InitState() => StartLoading();

        public override void DidUpdateWidget(Image oldWidget)
        {
            if (oldWidget.Provider != Widget!.Provider)
            {
                StartLoading();
            }
        }

        public override Widget Build(IBuildContext context) => new TaskBuilder<SkiaSharp.SKImage>
        {
            Task = _loadTask,
            Builder = (_, snapshot) => BuildSnapshot(snapshot)
        };

        /// <summary>
        /// このStateがツリーから外れるときに、読み込み済みの画像を解放する。
        /// SKImageはネイティブメモリを持つためGC任せにはできない。
        /// </summary>
        public override void Dispose()
        {
            DisposeLoadedImage(_loadTask);
            base.Dispose();
        }

        private void StartLoading()
        {
            // Providerが差し替わった場合、前回読み込んだ画像はもう誰も参照しないためここで解放する。
            DisposeLoadedImage(_loadTask);
            _reportedError = false;
            _loadTask = Widget!.Provider.LoadAsync().AsTask();
        }

        /// <summary>
        /// 読み込み済みなら画像を破棄する。未完了のタスクは完了後に破棄されるよう継続を登録する。
        /// </summary>
        private static void DisposeLoadedImage(Task<SkiaSharp.SKImage>? task)
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

        private Widget BuildSnapshot(TaskSnapshot<SkiaSharp.SKImage> snapshot)
        {
            // 読み込み失敗をここで再スローするとBuildScope→DrawFrameを貫通し、
            // FloatSodaApp.MainLoopのcatchでアプリ全体が停止してしまう。
            // 1枚の画像の失敗を全画面消失に広げないため、Child(またはプレースホルダー)へフォールバックする。
            if (snapshot.HasError && !_reportedError)
            {
                _reportedError = true;
                Widget!.OnError?.Invoke(snapshot.Error!);
            }

            return snapshot.HasData
                ? new ResolvedImage
                {
                    Image = snapshot.Data!,
                    Fit = Widget!.Fit,
                    Alignment = Widget!.Alignment,
                    Child = Widget!.Child
                }
                : new SizedBox { Child = Widget!.Child };
        }
    }

    private sealed record ResolvedImage : SingleChildRenderObjectWidget<RenderImage>
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
