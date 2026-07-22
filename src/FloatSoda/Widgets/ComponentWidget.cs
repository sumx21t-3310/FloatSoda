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

public abstract record StatefulWidget<T> : Widget where T : StatefulWidget<T>
{
    public override Element CreateElement() => new StatefulElement<T>
    {
        Widget = this
    };

    public abstract State<T> CreateState();
}

public abstract class State<T> where T : StatefulWidget<T>
{
    public T? Widget { get; set; }

    public StatefulElement<T>? Element { get; set; }
    public IBuildContext Context => Element!;

    public virtual void InitState() { }

    protected virtual void SetState(Action action)
    {
        action();
        Element?.MarkNeedsBuild();
    }

    public virtual void DidUpdateWidget(T oldWidget) { }

    public virtual void DidChangeDependencies() { }

    public abstract Widget Build(IBuildContext context);
}