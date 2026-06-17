using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record StatelessWidget : Widget
{
    public override Element CreateElement() => new StatelessElement
    {
        Widget = this
    };

    public abstract Widget Build(IBuildContext context);
}