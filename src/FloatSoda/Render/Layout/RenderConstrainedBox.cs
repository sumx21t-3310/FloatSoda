using FloatSoda.Geometrics;

namespace FloatSoda.Render.Layout;

public class RenderConstrainedBox : RenderProxyBox
{
    public required BoxConstraints AdditionalConstraints { get; init; }

    public RenderConstrainedBox() => Child?.ParentData = new BoxParentData();

    public override void PerformLayout()
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(Constraints);
        Child?.Layout(enforcedConstraints);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }
}