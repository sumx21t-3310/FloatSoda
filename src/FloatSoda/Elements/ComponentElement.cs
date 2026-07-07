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


    public override void PerformRebuild()
    {
        var built = Build();
        Dirty = false;
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


    public override Widget Build()
    {
        throw new NotImplementedException();
    }
}

public class StatelessElement : ComponentElement
{
    public override Widget Build() => ((StatelessWidget)Widget!).Build(this);
}