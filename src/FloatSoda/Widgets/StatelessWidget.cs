using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record StatelessWidget : Widget
{
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
    
    public abstract Widget Build(IBuildContext context);
}