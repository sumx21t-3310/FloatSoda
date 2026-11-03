using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Elements;

public class BuildOwnerTest
{
    private class BuildLog
    {
        public int OuterBuildCount;
        public int InnerBuildCount;
    }

    private record OuterWidget : StatelessWidget
    {
        public required BuildLog Log { get; init; }

        public override Widget Build(IBuildContext context)
        {
            Log.OuterBuildCount++;
            return new InnerWidget { Log = Log, Generation = Log.OuterBuildCount };
        }
    }

    private record InnerWidget : StatelessWidget
    {
        public required BuildLog Log { get; init; }
        public required int Generation { get; init; }

        public override Widget Build(IBuildContext context)
        {
            Log.InnerBuildCount++;
            return new SizedBox();
        }
    }

    [Fact]
    public void BuildScope_祖先の再構築で更新済みの子_もう一度構築しない()
    {
        var log = new BuildLog();
        var owner = new BuildOwner(() => { });
        var root = new RenderObjectToWidgetAdapter
        {
            Container = new RenderView(),
            Child = new OuterWidget { Log = log },
        }.AttachToRenderTree(owner, null);
        var outer = FindChild(root);
        var inner = FindChild(outer);
        Assert.Equal(1, log.InnerBuildCount);

        inner.MarkNeedsBuild();
        outer.MarkNeedsBuild();
        owner.BuildScope();

        Assert.Equal(2, log.OuterBuildCount);
        Assert.Equal(2, log.InnerBuildCount);
    }

    private static Element FindChild(Element element)
    {
        Element? child = null;
        element.VisitChildren(c => child = c);
        return child ?? throw new InvalidOperationException("子Elementが見つかりません。");
    }
}
