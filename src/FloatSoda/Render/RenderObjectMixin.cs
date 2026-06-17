namespace FloatSoda.Render;

public interface IHasSingleChildRenderObject
{
    void SetChildObject(RenderObject child);
}

public interface IHasSingleChildRenderObject<T> : IHasSingleChildRenderObject where T : RenderObject
{
    T? Child { get; protected set; }

    RenderObject ThisRef { get; }

    public void SetChild(T child)
    {
        Child = child;
        ThisRef.AdoptChild(child);
    }

    void IHasSingleChildRenderObject.SetChildObject(RenderObject child)
    {
        if (child is T typed) SetChild(typed);
    }
}

public interface IHasMultiChildrenRenderObject<T> where T : RenderObject
{
    List<T> Children { get; }
    RenderObject ThisRef { get; }

    void Insert(T child)
    {
        ThisRef.AdoptChild(child);
        Children.Add(child);
    }


    void VisitChildren(Action<RenderObject> visitor) => Children.ForEach(visitor);
}