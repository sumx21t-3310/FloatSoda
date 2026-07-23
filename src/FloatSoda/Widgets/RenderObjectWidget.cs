using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

/// <summary>
/// RenderObjectの生成と更新に必要な構成を宣言するウィジェットの基底型です。
/// </summary>
/// <typeparam name="T">このウィジェットが構成するRenderObjectの型。</typeparam>
public abstract record RenderObjectWidget<T> : Widget where T : RenderObject
{
    /// <summary>
    /// Elementが初めてマウントされるときに、このウィジェットの構成に対応するRenderObjectを生成します。
    /// </summary>
    /// <returns>RenderObjectツリーへ接続する新しいRenderObject。</returns>
    public abstract T CreateRenderObject();

    /// <summary>
    /// 既存のElementが再利用されたときに、現在のウィジェットの構成をRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">
    /// <see cref="CreateRenderObject"/>によって生成され、Elementが現在保持しているRenderObject。
    /// </param>
    /// <remarks>
    /// 既定の実装は何も変更しません。派生型は、構成値の変更に応じて必要なDirty状態が更新されるよう、
    /// RenderObjectの公開プロパティを介して差分を反映します。
    /// </remarks>
    public virtual void UpdateRenderObject(T renderObject) {}
}