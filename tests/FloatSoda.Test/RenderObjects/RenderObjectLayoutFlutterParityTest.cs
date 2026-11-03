using FloatSoda.Geometrics;
using FloatSoda.Geometry;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Test.RenderObjects;

public class RenderObjectLayoutFlutterParityTest
{
    [Fact]
    public void Layout_制約不変でRelayoutBoundaryだけ変更_PerformLayoutを繰り返さない()
    {
        var child = new LayoutCountingBox();
        var parent = new RenderConstrainedBox { Child = child };
        parent.RelayoutBoundary = parent;
        var constraints = new BoxConstraints(MaxWidth: 100, MaxHeight: 100);
        child.Layout(constraints);
        Assert.Equal(1, child.LayoutCount);

        child.Layout(constraints, parentUseSize: true);

        Assert.Same(parent, child.RelayoutBoundary);
        Assert.Equal(1, child.LayoutCount);
    }

    private sealed class LayoutCountingBox : RenderBox
    {
        public int LayoutCount { get; private set; }

        public override void PerformLayout()
        {
            LayoutCount++;
            Size = Constraints.Smallest;
        }

        public override void Paint(PaintingContext context, Offset offset) { }
    }
}
