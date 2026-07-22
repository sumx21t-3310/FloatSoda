using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Gesture;

public class RenderIgnorePointer : RenderProxyBox
{
    public bool Ignoring { get; set; }

    public override bool HitTest(HitTestResult result, Offset position) => !Ignoring && base.HitTest(result, position);
}
