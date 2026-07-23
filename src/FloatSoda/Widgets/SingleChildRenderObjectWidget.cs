using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

/// <summary>
/// 1つの子ウィジェットを持つRenderObjectの構成を宣言するウィジェットの基底型です。
/// </summary>
/// <typeparam name="T">このウィジェットが構成するRenderObjectの型。</typeparam>
public abstract record SingleChildRenderObjectWidget<T> : RenderObjectWidget<T> where T : RenderObject
{
    /// <summary>
    /// RenderObjectの子として接続するウィジェットを取得します。
    /// <see langword="null"/>の場合、子を持たない構成として扱われます。
    /// </summary>
    public Widget? Child { get; init; }

    /// <inheritdoc/>
    public override Element CreateElement() => new SingleChildRenderObjectElement<T>
    {
        Widget = this
    };
}