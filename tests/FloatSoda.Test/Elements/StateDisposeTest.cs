using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Elements;

public class StateDisposeTest
{
    private class Holder
    {
        public bool ShowChild = true;
        public int ChildDisposeCount;
        public int GrandChildDisposeCount;
        public RootState? Root;
    }

    private record RootWidget : StatefulWidget<RootWidget>
    {
        public required Holder Holder { get; init; }
        public override State<RootWidget> CreateState() => new RootState();
    }

    private class RootState : State<RootWidget>
    {
        public override void InitState() => Widget!.Holder.Root = this;

        public void Toggle() => SetState(() => Widget!.Holder.ShowChild = !Widget!.Holder.ShowChild);

        public override Widget Build(IBuildContext context)
            => Widget!.Holder.ShowChild
                ? new ChildWidget { Holder = Widget!.Holder }
                : new SizedBox { Width = 1, Height = 1 };
    }

    private record ChildWidget : StatefulWidget<ChildWidget>
    {
        public required Holder Holder { get; init; }
        public override State<ChildWidget> CreateState() => new ChildState();
    }

    private class ChildState : State<ChildWidget>
    {
        public override void Dispose()
        {
            Widget!.Holder.ChildDisposeCount++;
            base.Dispose();
        }

        public override Widget Build(IBuildContext context)
            => new GrandChildWidget { Holder = Widget!.Holder };
    }

    private record GrandChildWidget : StatefulWidget<GrandChildWidget>
    {
        public required Holder Holder { get; init; }
        public override State<GrandChildWidget> CreateState() => new GrandChildState();
    }

    private class GrandChildState : State<GrandChildWidget>
    {
        public override void Dispose()
        {
            Widget!.Holder.GrandChildDisposeCount++;
            base.Dispose();
        }

        public override Widget Build(IBuildContext context) => new SizedBox { Width = 5, Height = 5 };
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
    public void Dispose_NotCalled_WhileMounted()
    {
        var holder = new Holder();
        MountTree(new RootWidget { Holder = holder });

        Assert.Equal(0, holder.ChildDisposeCount);
        Assert.Equal(0, holder.GrandChildDisposeCount);
    }

    [Fact]
    public void Dispose_Called_WhenRemovedFromTree()
    {
        var holder = new Holder();
        var (_, owner) = MountTree(new RootWidget { Holder = holder });

        holder.Root!.Toggle();   // 子サブツリーを SizedBox へ差し替え → 旧サブツリーは非活性化
        owner.BuildScope();

        Assert.Equal(1, holder.ChildDisposeCount);
        Assert.Equal(1, holder.GrandChildDisposeCount);
    }

    [Fact]
    public void Dispose_CalledOnce_PerState()
    {
        var holder = new Holder();
        var (_, owner) = MountTree(new RootWidget { Holder = holder });

        holder.Root!.Toggle();
        owner.BuildScope();
        // 既に破棄済み。再ビルドしても二重には呼ばれない。
        owner.BuildScope();

        Assert.Equal(1, holder.ChildDisposeCount);
        Assert.Equal(1, holder.GrandChildDisposeCount);
    }
}
