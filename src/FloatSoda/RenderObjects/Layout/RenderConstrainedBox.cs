using FloatSoda.Geometrics;

namespace FloatSoda.RenderObjects.Layout;

public class RenderConstrainedBox : RenderProxyBox
{
    public BoxConstraints AdditionalConstraints
    {
        get;
        set
        {
            if (value == field) return;

            field = value;
            MarkNeedsLayout();
        }
    }

    public override void PerformLayout()
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(Constraints);
        Child?.Layout(enforcedConstraints, parentUseSize: true);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }
}