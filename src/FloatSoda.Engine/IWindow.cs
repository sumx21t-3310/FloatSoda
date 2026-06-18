using FloatSoda.Common.Layer;

namespace FloatSoda.Engine;

public interface IWindow : IDisposable
{
    string Key { get; }
    ILayer Root { get; set; }

    void Update();
}