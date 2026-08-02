using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

internal interface IParentDataElement
{
    void ApplyParentData(RenderObject renderObject);
}

/// <summary>
/// <see cref="ParentDataWidget{T}"/>の構成を、子孫のRenderObjectが保持するParentDataへ反映するElementです。
/// </summary>
/// <typeparam name="T">親RenderObjectが子ごとに用意するParentDataの型。</typeparam>
/// <seealso cref="ParentDataWidget{T}"/>
public class ParentDataElement<T> : ProxyElement, IParentDataElement where T : class, IParentData
{
    /// <summary>
    /// Widget更新後の構成を子孫RenderObjectのParentDataへ反映します。
    /// </summary>
    /// <param name="newWidget">同じ位置を引き継ぐ新しいParentDataWidget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);

        if (RenderObject is { } renderObject)
        {
            ApplyParentData(renderObject);
        }
    }

    void IParentDataElement.ApplyParentData(RenderObject renderObject) => ApplyParentData(renderObject);

    private void ApplyParentData(RenderObject renderObject)
        => ((ParentDataWidget<T>)Widget!).ApplyParentData(renderObject);
}
