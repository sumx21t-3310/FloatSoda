namespace FloatSoda.Widgets.Components;

public record Button : StatefulWidget<Button>
{
    public Widget Child { get; init; }
    
    public Action OnPressed { get; init; }
    public override State<Button> CreateState() => new ButtonState();
}

public record ButtonState : State<Button>
{
    public override Widget Build(IBuildContext context)
    {
        throw new NotImplementedException();
    }
}