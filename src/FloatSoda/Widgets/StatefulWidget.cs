using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record StatefulWidget : Widget
{
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }

    public abstract State<T> CreateState<T>() where T : StatefulWidget;
}

public record State<T> where T : StatefulWidget
{
}