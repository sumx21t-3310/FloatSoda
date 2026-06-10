using FloatSoda.Geometrics;

namespace FloatSoda.Widgets.Layout;

public record Center : StatelessWidget
{
    public Widget? Child { get; init; }

    public override Widget Build(IBuildContext context)
    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = Child
        };
    }
}