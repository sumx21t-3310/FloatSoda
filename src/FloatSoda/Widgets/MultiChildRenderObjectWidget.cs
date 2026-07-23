using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

/// <summary>
/// 順序を持つ複数の子ウィジェットをRenderObjectへ接続する構成を宣言するウィジェットの基底型です。
/// </summary>
/// <typeparam name="T">このウィジェットが構成するRenderObjectの型。</typeparam>
public abstract record MultiChildRenderObjectWidget<T> : RenderObjectWidget<T> where T : RenderObject
{
    /// <summary>
    /// RenderObjectへ先頭から順に接続する子ウィジェットの一覧を取得します。
    /// 既定値は空の一覧です。
    /// </summary>
    /// <remarks>
    /// 更新時はウィジェットのランタイム型とキーを使用して既存のElementとの差分が計算され、
    /// 再利用できない子だけが置き換えられます。
    /// </remarks>
    public List<Widget> Children { get; init; } = [];

    /// <inheritdoc/>
    public override Element CreateElement()
    {
        return new MultiChildRenderObjectElement<T>
        {
            Widget = this
        };
    }
}