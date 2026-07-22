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

public class StatelessElement : ComponentElement
{
    public override Widget Build() => ((StatelessWidget)Widget!).Build(this);

    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Dirty = true;
        Rebuild();
    }
}

public class StatefulElement<T> : ComponentElement where T : StatefulWidget<T>
{
    public State<T> State => _stateInternal!;
    private State<T>? _stateInternal = null;
    private bool _didChangeDependencies = false;

    /// <summary><see cref="State{T}.Dispose"/>をすでに呼び出したかどうか。</summary>
    private bool _stateDisposed = false;

    public override void Mount(Element? parent)
    {
        _stateInternal = ((StatefulWidget<T>)Widget!).CreateState();
        State.Element = this;
        State.Widget = (T)Widget!;
        base.Mount(parent);
    }

    public override void FirstBuild()
    {
        State.InitState();
        State.DidChangeDependencies();
        base.FirstBuild();
    }

    public override void PerformRebuild()
    {
        if (_didChangeDependencies)
        {
            State.DidChangeDependencies();
            _didChangeDependencies = false;
        }

        base.PerformRebuild();
    }


    public override Widget Build() => State.Build(this);

    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);

        var oldWidget = State.Widget!;
        Dirty = true;
        State.Widget = (T)Widget!;
        State.DidUpdateWidget(oldWidget);
        Rebuild();
    }

    public override void DidChangeDependencies()
    {
        base.DidChangeDependencies();
        _didChangeDependencies = true;
    }

    /// <summary>
    /// このElement(＝State)がツリーから恒久的に取り除かれるときに呼ばれる。
    /// 子を先に非活性化してから自身のStateを破棄することで、深い側から順に解放される。
    /// 再活性プールを持たない現状のツリーでは Deactivate が終端(unmount相当)となる。
    /// </summary>
    protected override void Deactivate()
    {
        base.Deactivate();

        if (_stateDisposed) return;
        _stateDisposed = true;
        State.Dispose();
    }
}
