using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;

namespace FloatSoda.Render.Layout;

public class RenderConstrainedBox : RenderProxyBox
{
    public required BoxConstraints AdditionalConstraints { get; init; }

    public RenderConstrainedBox() => Child?.ParentData = new BoxParentData();

    public override void Layout(BoxConstraints constraints)
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(constraints);
        Child?.Layout(enforcedConstraints);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}