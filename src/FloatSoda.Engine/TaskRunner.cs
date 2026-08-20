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
public interface ITaskRunner
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
    /// 専用スレッドに設定される名前を取得します。
    /// </summary>
    string ThreadName { get; }

    /// <summary>
    /// 専用スレッドが開始済みで、かつ終了していないかどうかを取得します。
    /// </summary>
    bool IsRunning { get; }
}

/// <summary>
/// I/Oリソースに関するスレッド所属処理を、専用の単一バックグラウンドスレッドで順番に実行します。
/// </summary>
/// <remarks>
/// このランナーはフレーム周期を持ちません。投入された処理があるときだけ専用スレッドを起床し、
/// FIFO順に1件ずつ実行します。ファイルやネットワークの非同期I/O自体ではなく、
/// 特定スレッドで直列化する必要があるリソース生成やアップロード処理に使用してください。
/// </remarks>
public sealed class IOTaskRunner : ITaskRunner, IDisposable
{
    /// <summary>専用スレッドの終了を待つ上限。超過しても呼び出し元をブロックし続けない。</summary>
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(3);

    private readonly object _gate = new();
    private readonly ILogger? _logger;
    private DedicatedThreadTaskScheduler? _scheduler;

    /// <summary>
    /// 停止を要求したが、専用スレッドの終了をまだ確認できていないスケジューラー。
    /// 終了前に再開始すると前後2本のスレッドが同時にキューを処理するため、Startの判定に使う。
    /// </summary>
    private DedicatedThreadTaskScheduler? _stoppingScheduler;
    private TaskFactory? _taskFactory;
    private CancellationTokenRegistration _stopRegistration;
    private bool _disposed;

    /// <summary>
    /// I/Oタスクランナーを作成します。
    /// </summary>
    /// <param name="threadName">専用スレッドに設定する名前。</param>
    /// <param name="logger">停止待機のタイムアウトを記録するロガー。記録しない場合は <see langword="null"/>。</param>
    public IOTaskRunner(string threadName = "IOThread", ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(threadName);
        ThreadName = threadName;
        _logger = logger;
    }

    /// <summary>
    /// 専用スレッドに設定される名前を取得します。
    /// </summary>
    public string ThreadName { get; }

    /// <summary>
    /// 専用スレッドが開始済みで、かつ終了していないかどうかを取得します。
    /// </summary>
    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _scheduler?.IsRunning ?? false;
            }
        }
    }

    /// <summary>
    /// 呼び出し元がこのランナーの専用スレッドかどうかを取得します。
    /// </summary>
    public bool RunsTasksOnCurrentThread
    {
        get
        {
            lock (_gate)
            {
                return _scheduler?.RunsTasksOnCurrentThread ?? false;
            }
        }
    }

    /// <summary>
    /// 専用スレッドを開始します。すでに実行中の場合は何もしません。
    /// </summary>
    /// <exception cref="ObjectDisposedException">このインスタンスが破棄済みです。</exception>
    /// <exception cref="InvalidOperationException">
    /// 前回の専用スレッドが待機上限内に終了しませんでした。この状態で開始すると単一スレッドでの
    /// 直列実行が保証できないため、開始しません。
    /// </exception>
    /// <param name="token">専用スレッドの停止要求を通知するキャンセルトークン。</param>
    public void Start(CancellationToken token = default)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_scheduler is { IsRunning: true }) return;
        }

        // 停止トークンで受付を閉じた前回のスケジューラーがキューを消化中の場合、
        // その終了を待ってから新しい専用スレッドを開始する。
        //
        // 待機には上限があるため、終了を確認できないことがある。そのまま開始すると前後2本の
        // スレッドが同時にキューを処理し、このクラスが約束している「単一スレッドで投入順に1件ずつ」
        // が崩れる。黙って壊すより開始を失敗させる。
        var stopped = StopCore();

        lock (_gate)
        {
            // 自スレッドからの停止では終了を待てないため、_stoppingSchedulerが残る。
            // 実際にスレッドが終了していれば、その事実をもって開始を許可する。
            if (_stoppingScheduler is { IsThreadAlive: false }) _stoppingScheduler = null;

            if (!stopped || _stoppingScheduler is not null)
            {
                throw new InvalidOperationException(
                    $"{ThreadName}の前回のスレッドが終了していないため、開始できません。投入済みの処理の完了を待ってから再度開始してください。");
            }
        }

        DedicatedThreadTaskScheduler scheduler;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_scheduler is { IsRunning: true }) return;

            scheduler = new DedicatedThreadTaskScheduler(ThreadName);
            _scheduler = scheduler;
            _taskFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                TaskContinuationOptions.None,
                scheduler);
            scheduler.Start();
        }

        var registration = token.UnsafeRegister(
            static state =>
            {
                var (runner, targetScheduler) = ((IOTaskRunner, DedicatedThreadTaskScheduler))state!;
                runner.RequestStop(targetScheduler);
            },
            (this, scheduler));

        lock (_gate)
        {
            if (ReferenceEquals(_scheduler, scheduler))
            {
                _stopRegistration = registration;
            }
            else
            {
                registration.Dispose();
            }
        }
    }

    /// <summary>
    /// 新しいタスクの受付を停止し、投入済みの処理を完了してから専用スレッドを終了します。
    /// </summary>
    /// <remarks>
    /// 専用スレッド自身から呼び出した場合は終了待機を行いません。
    /// 投入済みの処理が長引く場合、待機は3秒で打ち切って警告を記録します。
    /// </remarks>
    public void Stop() => StopCore();

    /// <summary>
    /// 停止処理の本体。専用スレッドの終了を確認できた場合に <see langword="true"/> を返す。
    /// 戻り値は再開始の可否判定に使う。
    /// </summary>
    private bool StopCore()
    {
        DedicatedThreadTaskScheduler? scheduler;
        CancellationTokenRegistration registration;
        lock (_gate)
        {
            scheduler = _scheduler;
            _scheduler = null;
            _taskFactory = null;
            registration = _stopRegistration;
            _stopRegistration = default;
            scheduler?.Complete();
            if (scheduler is not null) _stoppingScheduler = scheduler;
        }

        registration.Dispose();

        var exited = WaitForExit(scheduler);
        if (!exited) return false;

        lock (_gate)
        {
            if (ReferenceEquals(_stoppingScheduler, scheduler)) _stoppingScheduler = null;
        }

        return true;
    }

    /// <summary>
    /// 専用スレッドの終了を上限つきで待つ。上限を超えた場合は警告を記録し、呼び出し元へ制御を返す。
    /// 投入済みの処理には中断手段が無く、無期限に待つとアプリケーションの終了自体が止まるため。
    /// </summary>
    /// <returns>終了を確認できた場合は <see langword="true"/>。上限を超えた場合は <see langword="false"/>。</returns>
    private bool WaitForExit(DedicatedThreadTaskScheduler? scheduler)
    {
        if (scheduler is null) return true;

        // 専用スレッド自身からの停止では、自スレッドをJoinするとデッドロックするため待てない。
        // 待てない以上「終了した」とは言えないので、確認できなかったものとして扱う。
        // ここでtrueを返すと、実行中のタスクの中から呼ばれたStartが2本目のスレッドを立ててしまう。
        if (scheduler.RunsTasksOnCurrentThread) return false;

        if (scheduler.WaitForExit(StopTimeout)) return true;

        _logger?.LogWarning("{ThreadName} の停止がタイムアウトしました", ThreadName);
        return false;
    }

    private void RequestStop(DedicatedThreadTaskScheduler targetScheduler)
    {
        lock (_gate)
        {
            if (!ReferenceEquals(_scheduler, targetScheduler)) return;

            _taskFactory = null;
            targetScheduler.Complete();
        }
    }

    /// <summary>
    /// 戻り値を持たない処理を専用スレッドへ投入します。
    /// </summary>
    /// <param name="action">専用スレッドで実行する同期処理。</param>
    /// <param name="cancellationToken">処理の開始前にキャンセルするためのトークン。</param>
    /// <returns>処理の完了、例外、またはキャンセルを表すタスク。</returns>
    public Task RunAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Schedule(action, cancellationToken);
    }

    /// <summary>
    /// 戻り値を持つ処理を専用スレッドへ投入します。
    /// </summary>
    /// <typeparam name="T">処理が返す値の型。</typeparam>
    /// <param name="function">専用スレッドで実行する同期処理。</param>
    /// <param name="cancellationToken">処理の開始前にキャンセルするためのトークン。</param>
    /// <returns>処理の結果、例外、またはキャンセルを表すタスク。</returns>
    public Task<T> RunAsync<T>(Func<T> function, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(function);
        return Schedule(function, cancellationToken);
    }

    private Task Schedule(Action action, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            ThrowIfUnavailable();
            return _taskFactory!.StartNew(action, cancellationToken);
        }
    }

    private Task<T> Schedule<T>(Func<T> function, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            ThrowIfUnavailable();
            return _taskFactory!.StartNew(function, cancellationToken);
        }
    }

    private void ThrowIfUnavailable()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_taskFactory is null)
        {
            throw new InvalidOperationException("IOTaskRunnerが開始されていません。");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DedicatedThreadTaskScheduler? scheduler;
        CancellationTokenRegistration registration;
        lock (_gate)
        {
            if (_disposed) return;

            _disposed = true;
            scheduler = _scheduler;
            _scheduler = null;
            _taskFactory = null;
            registration = _stopRegistration;
            _stopRegistration = default;
            scheduler?.Complete();
        }

        registration.Dispose();
        // 破棄時は上限超過でも続行する。呼び出し元へ制御を返さないとアプリケーションの終了が止まる。
        _ = WaitForExit(scheduler);
    }

    private sealed class DedicatedThreadTaskScheduler : TaskScheduler
    {
        private readonly BlockingCollection<Task> _tasks = new(new ConcurrentQueue<Task>());
        private readonly Thread _thread;

        public DedicatedThreadTaskScheduler(string threadName)
        {
            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = threadName
            };
        }

        public bool IsRunning => _thread.IsAlive && !_tasks.IsAddingCompleted;

        public bool RunsTasksOnCurrentThread => Thread.CurrentThread == _thread;

        public void Start() => _thread.Start();

        public void Complete() => _tasks.CompleteAdding();

        public bool IsThreadAlive => _thread.IsAlive;

        public bool WaitForExit(TimeSpan timeout) => _thread.Join(timeout);

        protected override void QueueTask(Task task)
        {
            try
            {
                _tasks.Add(task);
            }
            catch (InvalidOperationException exception)
            {
                throw new TaskSchedulerException("IOTaskRunnerは停止済みです。", exception);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

        protected override IEnumerable<Task>? GetScheduledTasks() => _tasks.ToArray();

        private void Run()
        {
            foreach (var task in _tasks.GetConsumingEnumerable())
            {
                TryExecuteTask(task);
            }
        }
    }
}

/// <summary>
/// 一定のフレーム間隔でライフサイクルフックを呼び出す専用バックグラウンドスレッドを管理します。
/// </summary>
/// <param name="threadName">専用スレッドに設定する名前。</param>
/// <param name="pacer">各更新の間隔を制御するフレーム待機機構。</param>
/// <param name="logger">専用スレッド上の失敗と停止タイムアウトを記録するロガー。記録しない場合は <see langword="null"/>。</param>
public abstract class PostTaskRunner(string threadName, IFramePacer pacer, ILogger? logger = null) : ITaskRunner
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

    /// <summary>
    /// 専用スレッドで実行する処理をキューへ追加します。
    /// </summary>
    /// <param name="action">専用スレッドで実行する処理。</param>
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
    protected virtual void OnStart(CancellationToken ct) { }

    /// <summary>
    /// 各更新の直前に、専用スレッド上で前処理を実行します。
    /// </summary>
    protected virtual void PreUpdate() { }

    /// <summary>
    /// 各フレームの本処理を専用スレッド上で実行します。
    /// </summary>
    protected abstract void Update();

    /// <summary>
    /// 各更新の直後に、専用スレッド上で後処理を実行します。
    /// </summary>
    protected virtual void PostUpdate() { }

    /// <summary>
    /// 更新ループの終了時に、専用スレッド上で終了処理を実行します。
    /// </summary>
    /// <remarks>
    /// <see cref="OnStart(CancellationToken)"/> 以降に生成したスレッド所属リソースは、このメソッドで解放してください。
    /// </remarks>
    protected virtual void OnStop() { }
}

/// <summary>
/// GLFWイベントの処理とウィンドウへの描画を、GLFW/OpenGLコンテキストを所有するレンダースレッドで実行します。
/// </summary>
/// <param name="threadName">レンダースレッドに設定する名前。</param>
/// <param name="pacer">描画ループの更新間隔を制御するフレーム待機機構。</param>
/// <param name="logger">レンダースレッド上の失敗を記録するロガー。記録しない場合は <see langword="null"/>。</param>
public class RenderPostTaskRunner(string threadName, IFramePacer pacer, ILogger? logger = null)
    : PostTaskRunner(threadName, pacer, logger)
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
