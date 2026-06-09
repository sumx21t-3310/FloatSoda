namespace FloatSoda.Widgets.Components;

public record Text(string Data) : StatelessWidget 
{
    public override Widget Build(IBuildContext context)
    {
        return new RichText();
    }
}