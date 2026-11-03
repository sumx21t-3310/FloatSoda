using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometry;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class ColumnFlutterParityTest
{
    [Fact]
    public void Build_Columnの交差軸配置を省略_子を中央へ配置する()
    {
        var view = Mount(
            new SizedBox
            {
                Width = 100,
                Height = 100,
                Child = new Column
                {
                    Children =
                    {
                        new SizedBox { Width = 20, Height = 20 },
                    },
                },
            }
        );

        var flex = Find<RenderFlex>(view);
        var child = Assert.Single(flex.Children);

        Assert.Equal(new Offset(40, 0), Assert.IsType<FlexParentData>(child.ParentData).Offset);
    }

    private static RenderView Mount(Widget widget)
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        _ = new RenderObjectToWidgetAdapter { Container = view, Child = widget }.AttachToRenderTree(
            new BuildOwner(() => { }),
            null
        );
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        return view;
    }

    private static T Find<T>(RenderObject root)
        where T : RenderObject
    {
        T? found = null;
        Visit(root);
        return found ?? throw new InvalidOperationException($"{typeof(T).Name} が見つかりません。");

        void Visit(RenderObject node)
        {
            if (found is not null)
                return;
            if (node is T match)
            {
                found = match;
                return;
            }

            node.VisitChildren(Visit);
        }
    }
}
