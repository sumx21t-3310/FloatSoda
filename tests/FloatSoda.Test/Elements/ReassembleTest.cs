using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Elements;

public class ReassembleTest
{
    private class BuildLog
    {
        public int StatelessBuildCount;
        public int StatefulBuildCount;
        public int InitStateCount;
        public int LastFieldSeen;
    }

    private record OuterWidget : StatelessWidget
    {
        public required BuildLog Log { get; init; }

        public override Widget Build(IBuildContext context)
        {
            Log.StatelessBuildCount++;
            return new InnerWidget { Log = Log };
        }
    }

    private record InnerWidget : StatefulWidget<InnerWidget>
    {
        public required BuildLog Log { get; init; }

        public override State<InnerWidget> CreateState() => new InnerState();
    }

    private record InnerState : State<InnerWidget>
    {
        private int _field;

        public override void InitState()
        {
            Widget!.Log.InitStateCount++;
            _field = 42;
        }

        public override Widget Build(IBuildContext context)
        {
            Widget!.Log.StatefulBuildCount++;
            Widget!.Log.LastFieldSeen = _field;
            return new SizedBox { Width = 10, Height = 10 };
        }
    }

    private static (RenderObjectToWidgetElement<RenderView> Root, BuildOwner Owner) MountTree(Widget widget)
    {
        var renderView = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };

        var owner = new BuildOwner(() => { });

        var root = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);

        return (root, owner);
    }

    [Fact]
    public void Reassemble_RebuildsAllComponentElements()
    {
        var log = new BuildLog();
        var (root, owner) = MountTree(new OuterWidget { Log = log });

        Assert.Equal(1, log.StatelessBuildCount);
        Assert.Equal(1, log.StatefulBuildCount);

        root.Reassemble();
        owner.BuildScope();

        // record等価でBuild結果が変わらなくても、全ComponentElementのBuild()が再実行される
        Assert.Equal(2, log.StatelessBuildCount);
        Assert.Equal(2, log.StatefulBuildCount);
    }

    [Fact]
    public void Reassemble_PreservesState()
    {
        var log = new BuildLog();
        var (root, owner) = MountTree(new OuterWidget { Log = log });

        root.Reassemble();
        owner.BuildScope();

        // Stateは再生成されず（InitStateは初回のみ）、フィールド値も保持される
        Assert.Equal(1, log.InitStateCount);
        Assert.Equal(42, log.LastFieldSeen);
    }

    [Fact]
    public void Reassemble_IsIdempotentAcrossFrames()
    {
        var log = new BuildLog();
        var (root, owner) = MountTree(new OuterWidget { Log = log });

        root.Reassemble();
        owner.BuildScope();
        root.Reassemble();
        owner.BuildScope();

        Assert.Equal(3, log.StatelessBuildCount);
        Assert.Equal(3, log.StatefulBuildCount);
        Assert.Equal(1, log.InitStateCount);
    }
}
