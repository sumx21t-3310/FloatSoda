using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Gesture;

public class RenderAbsorbPointer : RenderProxyBox
{
    public bool Absorbing { get; set; }

    public override bool HitTest(HitTestResult result, Offset position)
    {
        return Absorbing ? Size.Contains(position) : base.HitTest(result, position);
    }
}
