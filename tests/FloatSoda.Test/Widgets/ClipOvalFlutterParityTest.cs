using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Geometry;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Test.Widgets;

public class ClipOvalFlutterParityTest
{
    [Fact]
    public void HitTest_楕円の外側かつ外接矩形の内側_ヒット対象にしない()
    {
        var listener = new RenderPointerListener
        {
            Behaviour = HitTestBehaviour.Opaque,
            Child = new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(100, 100),
            },
        };
        var clip = new RenderClipOval { Child = listener };
        var view = Mount(clip);
        var result = new HitTestResult();

        view.HitTest(result, new Offset(1, 1));

        Assert.DoesNotContain(result.Path, entry => ReferenceEquals(entry.Target, listener));
    }

    private static RenderView Mount(RenderBox child)
    {
        var view = new RenderView { Child = child };
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        return view;
    }
}
