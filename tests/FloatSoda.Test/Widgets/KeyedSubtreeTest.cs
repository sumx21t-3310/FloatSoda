using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class KeyedSubtreeTest
{
    private record IdentityProbe : StatefulWidget<IdentityProbe>
    {
        public required int Version { get; init; }
        public required Action<Guid> OnBuild { get; init; }
        public required Action<Guid> OnDispose { get; init; }

        public override State<IdentityProbe> CreateState() => new IdentityProbeState();
    }

    private class IdentityProbeState : State<IdentityProbe>
    {
        private readonly Guid _identity = Guid.NewGuid();

        public override Widget Build(IBuildContext context)
        {
            Widget!.OnBuild(_identity);
            return new SizedBox { Width = 10, Height = 10 };
        }

        public override void Dispose() => Widget!.OnDispose(_identity);
    }

    [Fact]
    public void Key_同じキーで更新_StateのIdentityを保持する()
    {
        var builds = new List<Guid>();
        var disposals = new List<Guid>();
        var tree = Mount(CreateWidget(1, 1, builds, disposals));

        Reattach(tree, CreateWidget(1, 2, builds, disposals));

        Assert.Equal(2, builds.Count);
        Assert.Equal(builds[0], builds[1]);
        Assert.Empty(disposals);
    }

    [Fact]
    public void Key_異なるキーで更新_サブツリーを差し替える()
    {
        var builds = new List<Guid>();
        var disposals = new List<Guid>();
        var tree = Mount(CreateWidget(1, 1, builds, disposals));

        Reattach(tree, CreateWidget(2, 2, builds, disposals));

        Assert.Equal(2, builds.Count);
        Assert.NotEqual(builds[0], builds[1]);
        Assert.Equal([builds[0]], disposals);
    }

    [Fact]
    public void Child_Null_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new KeyedSubtree { Child = null! });
    }

    private static KeyedSubtree CreateWidget(
        int key,
        int version,
        List<Guid> builds,
        List<Guid> disposals) => new()
    {
        Key = new ValueKey<int>(key),
        Child = new IdentityProbe
        {
            Version = version,
            OnBuild = builds.Add,
            OnDispose = disposals.Add
        }
    };

    private static (RenderObjectToWidgetElement<RenderView> Root, BuildOwner Owner, RenderView View) Mount(Widget widget)
    {
        var view = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        var owner = new BuildOwner(() => { });
        var root = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = widget
        }.AttachToRenderTree(owner, null);

        return (root, owner, view);
    }

    private static void Reattach(
        (RenderObjectToWidgetElement<RenderView> Root, BuildOwner Owner, RenderView View) tree,
        Widget widget)
    {
        _ = new RenderObjectToWidgetAdapter
        {
            Container = tree.View,
            Child = widget
        }.AttachToRenderTree(tree.Owner, tree.Root);
        tree.Owner.BuildScope();
    }
}
