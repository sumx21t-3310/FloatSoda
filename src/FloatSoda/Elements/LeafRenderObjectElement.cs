using FloatSoda.RenderObjects;

namespace FloatSoda.Elements;

/// <summary>
/// 子Widgetを持たないRenderObjectWidgetをRenderObjectへ対応付けるElementです。
/// </summary>
/// <typeparam name="T">このElementが管理するRenderObjectの型。</typeparam>
/// <seealso cref="Widgets.LeafRenderObjectWidget{T}"/>
public class LeafRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    /// <inheritdoc/>
    public override RenderObject? RenderObject { get; protected set; }
}
