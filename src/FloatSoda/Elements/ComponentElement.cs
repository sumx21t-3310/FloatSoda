using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class ComponentElement : Element
{
    private Element? Child { get; set; }

    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        FirstBuild();
    }

    public virtual void FirstBuild() => Rebuild();
    protected abstract void Rebuild();

    public override void PerformRebuild()
    {
        var built = Build();
        Child = UpdateChild(Child, built);
    }

    public abstract Widget Build();

    public override void VisitChildren(Action<Element> visitor)
    {
        if (Child != null) visitor(Child);
    }
}

public class StatefulElement : ComponentElement
{
    public void MarkNeedsBuild()
    {
        throw new NotImplementedException();
    }

    protected override void Rebuild()
    {
        throw new NotImplementedException();
    }

    public override Widget Build()
    {
        throw new NotImplementedException();
    }
}

public class StatelessElement : ComponentElement
{
    protected override void Rebuild() => PerformRebuild();

    public override Widget Build() => ((StatelessWidget)Widget!).Build(this);
}
