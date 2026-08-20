using FloatSoda.Engine;

namespace FloatSoda.Core.Providers;

/// <summary>外部または埋め込みデータから破棄可能なリソースを読み込む方法を表します。</summary>
/// <typeparam name="T">読み込むリソースの型。</typeparam>
public abstract record ResourceProvider<T> where T : IDisposable
{
    /// <summary>現在のFloatSoda実行コンテキストに関連付けられたI/Oランナーでリソースを読み込みます。</summary>
    /// <returns>呼び出し元が破棄するリソースオブジェクト。</returns>
    /// <remarks>
    /// I/Oランナーが関連付けられていない場合(<see cref="FloatSoda.Core.WidgetBinding"/>を直接生成した場合や
    /// <c>FloatSoda.Testing</c>のヘッドレスレンダラーなど)は、呼び出しスレッド上で同期的に読み込み、
    /// 完了済みの<see cref="ValueTask{TResult}"/>を返します。これにより1パスで
    /// ビルド・レイアウト・ペイントする経路でもリソースが確実に反映されます。
    /// 失敗とキャンセルは同期的に投げず、返すタスクの状態として通知します。
    /// </remarks>
    public ValueTask<T> LoadAsync(CancellationToken cancellationToken = default)
    {
        var taskRunner = ResourceProviderContext.CurrentTaskRunner;
        if (taskRunner is not null) return LoadAsync(taskRunner, cancellationToken);

        try
        {
            return new ValueTask<T>(LoadResource(cancellationToken));
        }
        catch (OperationCanceledException exception)
        {
            return new ValueTask<T>(Task.FromCanceled<T>(
                exception.CancellationToken.IsCancellationRequested ? exception.CancellationToken : new(true)));
        }
        catch (Exception exception)
        {
            return new ValueTask<T>(Task.FromException<T>(exception));
        }
    }

    /// <summary>指定したI/Oランナーでリソースを読み込みます。</summary>
    /// <param name="taskRunner">読み込み処理を実行するI/Oランナー。</param>
    /// <param name="cancellationToken">処理の開始前にキャンセルするためのトークン。</param>
    /// <returns>呼び出し元が破棄するリソースオブジェクト。</returns>
    public ValueTask<T> LoadAsync(IOTaskRunner taskRunner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskRunner);
        return new ValueTask<T>(taskRunner.RunAsync(() => LoadOnTaskRunner(cancellationToken), cancellationToken));
    }

    private T LoadOnTaskRunner(CancellationToken cancellationToken)
    {
        using var scope = ResourceProviderContext.Push(null);
        return Load(cancellationToken);
    }

    /// <summary>
    /// 呼び出しスレッド上でリソースを同期的に読み込みます。
    /// フォント解決のように同期APIから呼ばれ、I/Oランナーのキューを待てない経路で使用します。
    /// </summary>
    internal T LoadResource(CancellationToken cancellationToken = default)
    {
        using var scope = ResourceProviderContext.Push(null);
        return Load(cancellationToken);
    }

    /// <summary>リソースを同期的に読み込みます。FloatSodaランタイムからはI/Oランナー上で呼び出されます。</summary>
    /// <param name="cancellationToken">処理のキャンセルを通知するトークン。</param>
    /// <returns>読み込んだリソース。</returns>
    protected abstract T Load(CancellationToken cancellationToken);
}

internal static class ResourceProviderContext
{
    private static readonly AsyncLocal<IOTaskRunner?> TaskRunner = new();

    internal static IOTaskRunner? CurrentTaskRunner => TaskRunner.Value;

    internal static IDisposable Push(IOTaskRunner? taskRunner)
    {
        var previous = TaskRunner.Value;
        TaskRunner.Value = taskRunner;
        return new Scope(previous);
    }

    private sealed class Scope(IOTaskRunner? previous) : IDisposable
    {
        public void Dispose() => TaskRunner.Value = previous;
    }
}
