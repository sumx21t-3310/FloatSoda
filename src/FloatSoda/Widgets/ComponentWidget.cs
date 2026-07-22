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

    /// <summary>
    /// この State がツリーから恒久的に取り除かれるときに一度だけ呼ばれる。
    /// 購読・タイマー・認識器など、State が確保した外部リソースをここで解放する。
    /// </summary>
    public virtual void Dispose() { }

    public abstract Widget Build(IBuildContext context);
}