using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>Taskの接続状態を表します。</summary>
public enum TaskConnectionState
{
    /// <summary>監視するTaskがありません。</summary>
    None,

    /// <summary>Taskの完了を待機しています。</summary>
    Waiting,

    /// <summary>Taskが成功、失敗、またはキャンセルによって完了しました。</summary>
    Done
}

/// <summary>Taskの接続状態と、完了時の値または失敗を保持します。</summary>
/// <typeparam name="T">Taskが返す値の型。</typeparam>
public sealed record TaskSnapshot<T>
{
    /// <summary><typeparamref name="T"/>が<see langword="null"/>を取り得ない値型かどうか。</summary>
    private static readonly bool IsNonNullableValueType =
        typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) is null;

    /// <summary>Taskの現在の接続状態を取得します。</summary>
    public TaskConnectionState ConnectionState { get; init; }

    /// <summary>Taskが正常に完了した場合の値、または完了前の初期値を取得します。</summary>
    public T? Data { get; init; }

    /// <summary>Taskが失敗した場合の例外を取得します。</summary>
    public Exception? Error { get; init; }

    /// <summary>Taskがキャンセルされたかどうかを取得します。</summary>
    public bool IsCanceled { get; init; }

    /// <summary><see cref="Data"/>が有効な値を保持しているかどうかを取得します。</summary>
    /// <remarks>
    /// Taskが正常に完了した場合のほか、<see cref="TaskBuilder{T}.InitialData"/>が指定されていれば
    /// Taskの完了前でも<see langword="true"/>になります。
    /// <typeparamref name="T"/>が参照型で結果が<see langword="null"/>の場合は<see langword="false"/>です。
    /// </remarks>
    public bool HasData { get; init; }

    /// <summary>Taskが失敗したかどうかを取得します。</summary>
    public bool HasError => Error is not null;

    internal TaskSnapshot<T> InState(TaskConnectionState state) => this with { ConnectionState = state };

    internal static TaskSnapshot<T> FromCompletedTask(Task<T> task)
    {
        try
        {
            var data = task.GetAwaiter().GetResult();
            return new TaskSnapshot<T>
            {
                ConnectionState = TaskConnectionState.Done,
                Data = data,
                // 参照型でnullが返った場合にHasDataをtrueにすると、Data!の逆参照が呼び出し側で落ちる。
                HasData = IsNonNullableValueType || data is not null
            };
        }
        catch (OperationCanceledException) when (task.IsCanceled)
        {
            return new TaskSnapshot<T>
            {
                ConnectionState = TaskConnectionState.Done,
                IsCanceled = true
            };
        }
        catch (Exception exception)
        {
            return new TaskSnapshot<T>
            {
                ConnectionState = TaskConnectionState.Done,
                Error = exception
            };
        }
    }
}

/// <summary>Taskの現在の状態から、直下に配置するWidgetを構築する処理を表します。</summary>
/// <typeparam name="T">Taskが返す値の型。</typeparam>
/// <param name="context">このBuilderが配置されている構築コンテキスト。</param>
/// <param name="snapshot">Taskの現在の状態。</param>
/// <returns>Taskの現在の状態に対応するWidget。</returns>
public delegate Widget TaskWidgetBuilder<T>(IBuildContext context, TaskSnapshot<T> snapshot);

/// <summary>Taskの完了状態が変化するたびに、指定されたBuilderで直下のWidgetを再構築します。</summary>
/// <typeparam name="T">Taskが返す値の型。</typeparam>
/// <remarks>
/// TaskはこのWidgetより前のライフサイクルで作成し、同じ非同期処理には同じTaskインスタンスを渡してください。
/// Build中にTaskを作成すると、親の再構築ごとに処理が再開されます。
/// </remarks>
public record TaskBuilder<T> : StatefulWidget<TaskBuilder<T>>
{
    /// <summary>監視するTaskを取得します。Taskを監視しない場合は<see langword="null"/>です。</summary>
    public Task<T>? Task { get; init; }

    /// <summary>Taskが完了する前にSnapshotへ設定する初期値を取得します。</summary>
    /// <remarks>指定した場合、Taskの完了前でも<see cref="TaskSnapshot{T}.HasData"/>が<see langword="true"/>になります。</remarks>
    public T? InitialData
    {
        get;
        init
        {
            field = value;
            HasInitialData = true;
        }
    }

    /// <summary><see cref="InitialData"/>が明示的に指定されたかどうかを取得します。</summary>
    /// <remarks>値型の<typeparamref name="T"/>では既定値と未指定を区別できないため、指定の有無を別に保持します。</remarks>
    public bool HasInitialData { get; private init; }

    /// <summary>Taskの現在の状態から、直下に配置するWidgetを構築する処理を取得します。</summary>
    /// <exception cref="ArgumentNullException"><see langword="null"/>が指定されました。</exception>
    public required TaskWidgetBuilder<T> Builder
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <inheritdoc/>
    public override State<TaskBuilder<T>> CreateState() => new TaskBuilderState<T>();

    private sealed class TaskBuilderState<TValue> : State<TaskBuilder<TValue>>
    {
        private TaskSnapshot<TValue> _snapshot = new();
        private long _generation;

        public override void InitState()
        {
            _snapshot = new TaskSnapshot<TValue>
            {
                ConnectionState = TaskConnectionState.None,
                Data = Widget!.InitialData,
                HasData = Widget.HasInitialData
            };
            Subscribe(Widget.Task);
        }

        public override void DidUpdateWidget(TaskBuilder<TValue> oldWidget)
        {
            if (ReferenceEquals(oldWidget.Task, Widget!.Task)) return;

            _generation++;
            _snapshot = _snapshot.InState(TaskConnectionState.None);
            Subscribe(Widget.Task);
        }

        public override Widget Build(IBuildContext context) => Widget!.Builder(context, _snapshot);

        public override void Dispose()
        {
            _generation++;
            base.Dispose();
        }

        private void Subscribe(Task<TValue>? task)
        {
            if (task is null) return;

            var generation = ++_generation;
            if (task.IsCompleted)
            {
                _snapshot = TaskSnapshot<TValue>.FromCompletedTask(task);
                return;
            }

            _snapshot = _snapshot.InState(TaskConnectionState.Waiting);
            _ = task.ContinueWith(
                completedTask => Complete(completedTask, generation),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                Element!.Owner!.TaskScheduler);
        }

        private void Complete(Task<TValue> task, long generation)
        {
            if (generation != _generation) return;

            SetState(() => _snapshot = TaskSnapshot<TValue>.FromCompletedTask(task));
        }
    }
}
