using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

/// <summary>
/// 子RenderObjectへ、親RenderObjectのレイアウトで使用するParentDataを設定するWidgetの基底型です。
/// </summary>
/// <typeparam name="T">親RenderObjectが子ごとに用意するParentDataの型。</typeparam>
/// <remarks>
/// 派生Widgetは、対応する親RenderObjectが<see cref="RenderObject.SetupParentData"/>で
/// <typeparamref name="T"/>を設定する位置で使用します。
/// </remarks>
/// <seealso cref="ParentDataElement{T}"/>
public abstract record ParentDataWidget<T> : ProxyWidget where T : class, IParentData
{
    /// <inheritdoc/>
    public override Element CreateElement() => new ParentDataElement<T> { Widget = this };

    /// <summary>
    /// 現在のWidget構成を、子RenderObjectに用意されたParentDataへ反映します。
    /// </summary>
    /// <param name="renderObject">ParentDataを保持する子RenderObject。</param>
    /// <exception cref="InvalidOperationException">
    /// 親RenderObjectが<typeparamref name="T"/>を用意していない場合。
    /// </exception>
    public void ApplyParentData(RenderObject renderObject)
    {
        if (renderObject.ParentData is not T parentData)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} requires parent data of type {typeof(T).Name}, " +
                $"but {renderObject.GetType().Name} has {renderObject.ParentData?.GetType().Name ?? "no parent data"}.");
        }

        if (ApplyParentData(parentData))
        {
            renderObject.Parent?.MarkNeedsLayout();
        }
    }

    /// <summary>
    /// Widget固有の値をParentDataへ反映し、値が変更されたかを返します。
    /// </summary>
    /// <param name="parentData">親RenderObjectによって子へ用意されたParentData。</param>
    /// <returns>ParentDataを変更した場合は<see langword="true"/>。変更がない場合は<see langword="false"/>。</returns>
    /// <remarks>
    /// <see langword="true"/>を返すと、基底実装が親RenderObjectへレイアウト更新を要求します。
    /// </remarks>
    protected abstract bool ApplyParentData(T parentData);
}
