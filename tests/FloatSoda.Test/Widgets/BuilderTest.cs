using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class BuilderTest
{
    private record TestScope : InheritedWidget
    {
        public required int Value { get; init; }

        public override bool UpdateShouldNotify(InheritedWidget oldWidget) => !Equals(oldWidget, this);
    }

    [Fact]
    public void ChildBuilder_InheritedWidgetの子で構築_最も近いスコープを解決する()
    {
        TestScope? found = null;

        Mount(new TestScope
        {
            Value = 42,
            Child = new Builder
            {
                ChildBuilder = context =>
                {
                    found = context.DependOnInheritedWidgetOfExactType<TestScope>();
                    return new SizedBox { Width = 10, Height = 10 };
                }
            }
        });

        Assert.NotNull(found);
        Assert.Equal(42, found!.Value);
    }

    [Fact]
    public void ChildBuilder_Null_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new Builder { ChildBuilder = null! });
    }

    private static void Mount(Widget widget)
    {
        var renderView = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        var owner = new BuildOwner(() => { });

        _ = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);
    }
}
