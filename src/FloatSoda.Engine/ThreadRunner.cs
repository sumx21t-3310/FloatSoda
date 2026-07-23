using System.Collections.Concurrent;
using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Scheduling;
using FloatSoda.Rendering.Layers;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FloatSoda.Engine;

/// <summary>
/// 専用スレッドの開始、停止、およびタスク投入を統一する契約を定義します。
/// </summary>
public interface IThreadRunner
{
    /// <summary>
    /// 専用スレッドを開始します。すでに実行中の場合は新しいスレッドを開始しません。
    /// </summary>
    /// <param name="token">専用スレッドの停止要求を通知するキャンセルトークン。</param>
    void Start(CancellationToken token);

    /// <summary>
    /// 専用スレッドへ停止を要求し、呼び出し元がそのスレッド自身でない場合は終了を待機します。
    /// </summary>
    void Stop();

    /// <summary>
    /// 専用スレッドで実行する処理をキューへ追加します。
    /// </summary>
    /// <param name="action">専用スレッドで実行する処理。</param>
    void PostTask(Action action);

    /// <summary>
    /// 専用スレッドに設定される名前を取得します。
    /// </summary>
    string ThreadName { get; }

    /// <summary>
    /// 専用スレッドが開始済みで、かつ終了していないかどうかを取得します。
    /// </summary>
    bool IsRunning { get; }
}

/// <summary>
/// 一定のフレーム間隔でライフサイクルフックを呼び出す専用バックグラウンドスレッドを管理します。
/// </summary>
/// <param name="threadName">専用スレッドに設定する名前。</param>
/// <param name="pacer">各更新の間隔を制御するフレーム待機機構。</param>
/// <param name="logger">専用スレッド上の失敗と停止タイムアウトを記録するロガー。記録しない場合は <see langword="null"/>。</param>
public abstract class ThreadRunner(string threadName, IFramePacer pacer, ILogger? logger = null) : IThreadRunner
{
    private Thread? _thread;

    /// <summary>
    /// 派生クラスが専用スレッド上の状態や失敗を記録するためのロガーを取得します。
    /// </summary>
    protected ILogger? Logger => logger;

    private volatile bool _isRunning;
    private CancellationTokenSource? _linkedTokenSource;

    /// <inheritdoc />
    public bool IsRunning => _isRunning && (_thread?.IsAlive ?? false);


    /// <inheritdoc />
    public string ThreadName => threadName;

    /// <inheritdoc />
    public void Start(CancellationToken ct)
    {
        lock (this)
        {
            if (_thread is { IsAlive: true }) return;

            _isRunning = true;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _linkedTokenSource = cts;

            _thread = new Thread(() => RunLoop(cts.Token))
            {
                IsBackground = true,
                Name = ThreadName
            };

            _thread.Start();
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        Thread? targetThread;
        lock (this)
        {
            targetThread = _thread;
            _thread = null;
            _isRunning = false;
            _linkedTokenSource?.Cancel();
            _linkedTokenSource?.Dispose();
            _linkedTokenSource = null;
        }

        if (targetThread == null) return;

        if (Thread.CurrentThread == targetThread) return;

        if (!targetThread.Join(3000))
        {
            Logger?.LogWarning("{ThreadName} の停止がタイムアウトしました", ThreadName);
        }
    }

    private readonly ConcurrentQueue<Action> _pendingTasks = new();

    /// <inheritdoc />
    public virtual void PostTask(Action action) => _pendingTasks.Enqueue(action);

    /// <summary>
    /// キューに積まれたタスクを実行する。1タスクが例外を投げても他のタスクとスレッド自体は継続する。
    /// ここで拾わないと最上位の<see cref="RunLoop"/>のcatchまで抜けて<see cref="OnStop"/>が走り、
    /// レンダースレッドが停止して以降すべての描画が止まる。
    /// </summary>
    protected void DrainPendingTasks()
    {
        while (_pendingTasks.TryDequeue(out var task))
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "{ThreadName} でタスクの実行に失敗しました", ThreadName);
            }
        }
    }

    private void RunLoop(CancellationToken ct)
    {
        try
        {
            OnStart(ct);

            // _isRunning フラグとトークンの両方をチェック
            while (_isRunning && !ct.IsCancellationRequested)
            {
                PreUpdate();
                Update();
                PostUpdate();

                pacer.WaitForNextFrame(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常な中断
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in {ThreadName}", ThreadName);
        }
        finally
        {
            OnStop();
            _isRunning = false;
        }
    }

    /// <summary>
    /// 更新ループへ入る前に、専用スレッド上で初期化処理を実行します。
    /// </summary>
    /// <param name="ct">専用スレッドの停止要求を通知するキャンセルトークン。</param>
    /// <remarks>
    /// スレッドに所属するネイティブリソースは、このメソッドまたは以降のフックで生成してください。
    /// </remarks>
    protected virtual void OnStart(CancellationToken ct)
    {
    }

    /// <summary>
    /// 各更新の直前に、専用スレッド上で前処理を実行します。
    /// </summary>
    protected virtual void PreUpdate()
    {
    }

    /// <summary>
    /// 各フレームの本処理を専用スレッド上で実行します。
    /// </summary>
    protected abstract void Update();

    /// <summary>
    /// 各更新の直後に、専用スレッド上で後処理を実行します。
    /// </summary>
    protected virtual void PostUpdate()
    {
    }

    /// <summary>
    /// 更新ループの終了時に、専用スレッド上で終了処理を実行します。
    /// </summary>
    /// <remarks>
    /// <see cref="OnStart(CancellationToken)"/> 以降に生成したスレッド所属リソースは、このメソッドで解放してください。
    /// </remarks>
    protected virtual void OnStop()
    {
    }
}

/// <summary>
/// GLFWイベントの処理とウィンドウへの描画を、GLFW/OpenGLコンテキストを所有するレンダースレッドで実行します。
/// </summary>
/// <param name="threadName">レンダースレッドに設定する名前。</param>
/// <param name="pacer">描画ループの更新間隔を制御するフレーム待機機構。</param>
/// <param name="logger">レンダースレッド上の失敗を記録するロガー。記録しない場合は <see langword="null"/>。</param>
public class RenderThreadRunner(string threadName, IFramePacer pacer, ILogger? logger = null)
    : ThreadRunner(threadName, pacer, logger)
{
    /// <summary>
    /// 指定したレイヤーツリーをウィンドウへ反映する処理をレンダースレッドへ追加します。
    /// </summary>
    /// <param name="window">レイヤーツリーの反映先となるエンジンウィンドウ。</param>
    /// <param name="layer">反映するレイヤーツリー。</param>
    /// <remarks>
    /// 呼び出し元はレンダースレッドである必要はありません。実際の描画はレンダースレッド上で行われます。
    /// </remarks>
    public void PostRender(IEngineWindow window, ILayer layer)
    {
        PostTask(() =>
        {
            if (!IsRunning) return;

            window.Present(layer);
        });
    }

    /// <summary>
    /// レンダースレッドへ投入された描画処理とその他のタスクを、GLFWイベント処理の前に実行します。
    /// </summary>
    protected override void PreUpdate() => DrainPendingTasks();

    /// <summary>
    /// レンダースレッド上でGLFWを初期化し、ウィンドウとOpenGLリソースを生成できる状態にします。
    /// </summary>
    /// <param name="ct">初期化前の停止要求を通知するキャンセルトークン。</param>
    protected override void OnStart(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!GLFW.Init()) throw new Exception("GLFWの初期化に失敗しました。");
    }

    /// <summary>
    /// レンダースレッド上でGLFWイベントを処理します。
    /// </summary>
    protected override void Update() => GLFW.PollEvents();

    /// <summary>
    /// レンダースレッド上でGLFWを終了します。
    /// </summary>
    /// <remarks>
    /// GLFWウィンドウとOpenGLリソースは、このメソッドが呼ばれる前に同じレンダースレッド上で解放されている必要があります。
    /// </remarks>
    protected override void OnStop() => GLFW.Terminate();
}
