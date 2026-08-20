using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class TaskBuilderTest
{
    private sealed class SnapshotRecorder<T>
    {
        public List<TaskSnapshot<T>> Snapshots { get; } = [];
    }

    [Fact]
    public void InitState_Taskが未完了_Waitingで初回構築する()
    {
        var source = new TaskCompletionSource<int>();
        var recorder = new SnapshotRecorder<int>();

        Mount(new TaskBuilder<int>
        {
            Task = source.Task,
            InitialData = 10,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        var snapshot = Assert.Single(recorder.Snapshots);
        Assert.Equal(TaskConnectionState.Waiting, snapshot.ConnectionState);
        Assert.Equal(10, snapshot.Data);
        // InitialDataを指定した場合はTask完了前でもHasDataがtrueになる。
        Assert.True(snapshot.HasData);
    }

    [Fact]
    public void InitState_InitialData未指定でTaskが未完了_HasDataがfalseになる()
    {
        var source = new TaskCompletionSource<int>();
        var recorder = new SnapshotRecorder<int>();

        Mount(new TaskBuilder<int>
        {
            Task = source.Task,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        Assert.False(Assert.Single(recorder.Snapshots).HasData);
    }

    [Fact]
    public void Task_参照型でnullが返る_HasDataがfalseになる()
    {
        var recorder = new SnapshotRecorder<string>();

        Mount(new TaskBuilder<string>
        {
            Task = System.Threading.Tasks.Task.FromResult<string>(null!),
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        var snapshot = Assert.Single(recorder.Snapshots);
        Assert.Equal(TaskConnectionState.Done, snapshot.ConnectionState);
        Assert.False(snapshot.HasData);
    }

    [Fact]
    public void Task_成功_MainスレッドのBuildScopeで結果を反映する()
    {
        var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var recorder = new SnapshotRecorder<int>();
        using var taskScheduled = new ManualResetEventSlim();
        var (_, owner, _) = MountRoot(new TaskBuilder<int>
        {
            Task = source.Task,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        }, taskScheduled.Set);
        taskScheduled.Reset();

        var completionThread = new Thread(() => source.SetResult(42));
        completionThread.Start();
        completionThread.Join();
        Assert.True(taskScheduled.Wait(TimeSpan.FromSeconds(3)));

        Assert.Single(recorder.Snapshots);

        owner.BuildScope();

        Assert.Equal(2, recorder.Snapshots.Count);
        var snapshot = recorder.Snapshots[^1];
        Assert.Equal(TaskConnectionState.Done, snapshot.ConnectionState);
        Assert.Equal(42, snapshot.Data);
        Assert.True(snapshot.HasData);
    }

    [Fact]
    public void Task_失敗_Errorを保持して再構築する()
    {
        var source = new TaskCompletionSource<int>();
        var recorder = new SnapshotRecorder<int>();
        var owner = Mount(new TaskBuilder<int>
        {
            Task = source.Task,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });
        var error = new InvalidOperationException("boom");

        source.SetException(error);
        owner.BuildScope();

        var snapshot = recorder.Snapshots[^1];
        Assert.Equal(TaskConnectionState.Done, snapshot.ConnectionState);
        Assert.Same(error, snapshot.Error);
        Assert.True(snapshot.HasError);
        Assert.False(snapshot.HasData);
    }

    [Fact]
    public void Task_キャンセル_IsCanceledを保持して再構築する()
    {
        var source = new TaskCompletionSource<int>();
        var recorder = new SnapshotRecorder<int>();
        var owner = Mount(new TaskBuilder<int>
        {
            Task = source.Task,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        source.SetCanceled();
        owner.BuildScope();

        var snapshot = recorder.Snapshots[^1];
        Assert.Equal(TaskConnectionState.Done, snapshot.ConnectionState);
        Assert.True(snapshot.IsCanceled);
        Assert.False(snapshot.HasData);
        Assert.False(snapshot.HasError);
    }

    [Fact]
    public void InitState_Taskが完了済み_Doneで初回構築する()
    {
        var recorder = new SnapshotRecorder<int>();

        Mount(new TaskBuilder<int>
        {
            Task = System.Threading.Tasks.Task.FromResult(7),
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        var snapshot = Assert.Single(recorder.Snapshots);
        Assert.Equal(TaskConnectionState.Done, snapshot.ConnectionState);
        Assert.Equal(7, snapshot.Data);
    }

    [Fact]
    public void DidUpdateWidget_Task差し替え後に古いTaskが完了_結果を無視する()
    {
        var first = new TaskCompletionSource<int>();
        var second = new TaskCompletionSource<int>();
        var recorder = new SnapshotRecorder<int>();
        var (root, owner, renderView) = MountRoot(new TaskBuilder<int>
        {
            Task = first.Task,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        root = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = new TaskBuilder<int>
            {
                Task = second.Task,
                Builder = (_, snapshot) => Record(recorder, snapshot)
            }
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();

        first.SetResult(1);
        owner.BuildScope();
        Assert.DoesNotContain(recorder.Snapshots, snapshot => snapshot.Data == 1);

        second.SetResult(2);
        owner.BuildScope();
        Assert.Equal(2, recorder.Snapshots[^1].Data);
    }

    [Fact]
    public void Dispose_Task完了後も再構築しない()
    {
        var source = new TaskCompletionSource<int>();
        var recorder = new SnapshotRecorder<int>();
        var (root, owner, renderView) = MountRoot(new TaskBuilder<int>
        {
            Task = source.Task,
            Builder = (_, snapshot) => Record(recorder, snapshot)
        });

        _ = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = new SizedBox { Width = 1, Height = 1 }
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();
        var countAfterDispose = recorder.Snapshots.Count;

        source.SetResult(1);
        owner.BuildScope();

        Assert.Equal(countAfterDispose, recorder.Snapshots.Count);
    }

    [Fact]
    public void RequiredProperties_BuilderがNull_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new TaskBuilder<int>
        {
            Builder = null!
        });
    }

    private static Widget Record<T>(SnapshotRecorder<T> recorder, TaskSnapshot<T> snapshot)
    {
        recorder.Snapshots.Add(snapshot);
        return new SizedBox { Width = 1, Height = 1 };
    }

    private static BuildOwner Mount(Widget widget)
    {
        var (_, owner, _) = MountRoot(widget);
        return owner;
    }

    private static (RenderObjectToWidgetElement<RenderView> Root, BuildOwner Owner, RenderView RenderView) MountRoot(
        Widget widget,
        Action? onBuildScheduled = null)
    {
        var renderView = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        var owner = new BuildOwner(onBuildScheduled ?? (() => { }));
        var root = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);

        return (root, owner, renderView);
    }
}
