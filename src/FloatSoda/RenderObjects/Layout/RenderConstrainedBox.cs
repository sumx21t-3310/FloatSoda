using FloatSoda.Geometrics;

namespace FloatSoda.RenderObjects.Layout;

public class RenderConstrainedBox : RenderProxyBox
{
    public required BoxConstraints AdditionalConstraints { get; init; }

    public RenderConstrainedBox() => Child?.ParentData = new BoxParentData();

    public override void PerformLayout()
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(Constraints);
        Child?.Layout(enforcedConstraints, parentUseSize: true);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }
}