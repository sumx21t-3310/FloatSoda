namespace FloatSoda.Render;

public abstract class Element(Element? child)
{
    public Element? Child { get; init; } = child;


    public virtual void Draw(RenderContext context)
    {
        OnDraw(context);
        Child?.Draw(context);
    }

    public bool IsDirty { get; } = true;

    protected abstract void OnDraw(RenderContext context);
}