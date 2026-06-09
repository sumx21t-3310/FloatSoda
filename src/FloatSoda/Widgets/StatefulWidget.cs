using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record StatefulWidget<T> : Widget where T : StatefulWidget<T>
{
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }

    public abstract State<T> CreateState();
}

public abstract record State<T> where T : StatefulWidget<T>
{
    public abstract Widget Build(IBuildContext context);
}