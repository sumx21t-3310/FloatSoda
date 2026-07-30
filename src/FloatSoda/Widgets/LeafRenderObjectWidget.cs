using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

/// <summary>
/// 子Widgetを持たないRenderObjectの構成を宣言するウィジェットの基底型です。
/// </summary>
/// <typeparam name="T">このウィジェットが構成するRenderObjectの型。</typeparam>
public abstract record LeafRenderObjectWidget<T> : RenderObjectWidget<T> where T : RenderObject
{
    /// <inheritdoc/>
    public override Element CreateElement() => new LeafRenderObjectElement<T>
    {
        Widget = this
    };
}
