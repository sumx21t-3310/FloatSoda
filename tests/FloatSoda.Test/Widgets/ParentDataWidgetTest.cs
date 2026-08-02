using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ParentDataWidgetTest
{
    [Fact]
    public void ParentDataChange_MarksParentForLayoutAndChangesHeadlessLayoutResult()
    {
        var renderView = new RenderView();
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        var owner = new BuildOwner(() => { });
        renderView.PrepareInitialFrame();

        var root = Attach(
            width: 120,
            renderView,
            owner,
            element: null);

        pipeline.FlushLayout();

        Assert.Equal(new SKSize(120, 20), renderView.Size);

        root = Attach(
            width: 240,
            renderView,
            owner,
            root);
        owner.BuildScope();
        pipeline.FlushLayout();

        Assert.Equal(new SKSize(240, 20), renderView.Size);
    }

    private static RenderObjectToWidgetElement<RenderView> Attach(
        float width,
        RenderView renderView,
        BuildOwner owner,
        RenderObjectToWidgetElement<RenderView>? element)
    {
        return new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = new TestParentWidget
            {
                Child = new TestParentDataWidget
                {
                    Width = width,
                    Child = new TestProxyWidget
                    {
                        Child = new SizedBox { Width = 10, Height = 20 }
                    }
                }
            }
        }.AttachToRenderTree(owner, element);
    }

    private sealed record TestProxyWidget : StatelessWidget
    {
        public required Widget Child { get; init; }

        public override Widget Build(IBuildContext context) => Child;
    }

    private sealed class TestParentData : BoxParentData
    {
        public float Width { get; set; }
    }

    private sealed record TestParentDataWidget : ParentDataWidget<TestParentData>
    {
        public required float Width { get; init; }

        protected override bool ApplyParentData(TestParentData parentData)
        {
            if (parentData.Width == Width) return false;

            parentData.Width = Width;
            return true;
        }
    }

    private sealed record TestParentWidget : SingleChildRenderObjectWidget<TestParentRenderBox>
    {
        public override TestParentRenderBox CreateRenderObject() => new();
    }

    private sealed class TestParentRenderBox : RenderBox, IHasSingleChildRenderObject
    {
        private readonly SingleChildContainer<RenderBox> _child;

        public TestParentRenderBox() => _child = new SingleChildContainer<RenderBox>(this);

        private RenderBox? Child
        {
            get => _child.Child;
            set => _child.Child = value;
        }

        RenderObject? IHasSingleChildRenderObject.Child
        {
            get => Child;
            set => Child = (RenderBox?)value;
        }

        public override void SetupParentData(RenderObject child) => child.ParentData = new TestParentData();

        public override void PerformLayout()
        {
            if (Child is null)
            {
                Size = SKSize.Empty;
                return;
            }

            Child.Layout(Constraints, parentUseSize: true);
            var parentData = Assert.IsType<TestParentData>(Child.ParentData);
            Size = new SKSize(parentData.Width, Child.Size.Height);
        }

        public override void Paint(PaintingContext context, Offset offset)
        {
            if (Child is not null)
            {
                context.PaintChild(Child, offset);
            }
        }

        public override void Attach(RenderPipeline? owner)
        {
            base.Attach(owner);
            _child.Attach(owner);
        }

        public override void Detach()
        {
            base.Detach();
            _child.Detach();
        }

        public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

        public override void RedepthChildren() => VisitChildren(RedepthChild);
    }
}
