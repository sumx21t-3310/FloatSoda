using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 複数の子要素を主軸に沿って一列に配置します。
/// </summary>
/// <seealso cref="RenderFlex"/>
public sealed record Flex : MultiChildRenderObjectWidget<RenderFlex>
{
    /// <summary>
    /// 子要素を並べる主軸の方向を取得します。
    /// </summary>
    public Axis Direction { get; init; } = Axis.Vertical;
    /// <summary>
    /// 主軸方向における子要素の配置方法を取得します。
    /// </summary>
    public MainAxisAlignment MainAxisAlignment { get; init; } = MainAxisAlignment.Center;

    /// <summary>
    /// 主軸方向に確保する領域の大きさを取得します。
    /// </summary>
    public MainAxisSize MainAxisSize { get; init; } = MainAxisSize.Max;
    /// <summary>
    /// 交差軸方向における子要素の配置方法を取得します。
    /// </summary>
    public CrossAxisAlignment CrossAxisAlignment { get; init; } = CrossAxisAlignment.Center;
    /// <summary>
    /// 垂直方向の始端から終端へ向かう向きを取得します。
    /// </summary>
    public VerticalDirection VerticalDirection { get; init; } = VerticalDirection.Down;

    /// <summary>
    /// このウィジェットの軸方向と配置設定を保持するRenderObjectを生成します。
    /// </summary>
    /// <returns>複数の子要素を一列に配置する新しいRenderObject。</returns>
    public override RenderFlex CreateRenderObject()
    {
        return new RenderFlex
        {
            Direction = Direction,
            MainAxisAlignment = MainAxisAlignment,
            MainAxisSize = MainAxisSize,
            CrossAxisAlignment = CrossAxisAlignment,
            VerticalDirection = VerticalDirection,
        };
    }

    /// <summary>
    /// 軸方向と配置設定を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// いずれかの値が変更された場合、対象をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子要素のサイズと位置を再計算します。
    /// すべての値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderFlex renderObject)
    {
        renderObject.Direction = Direction;
        renderObject.MainAxisAlignment = MainAxisAlignment;
        renderObject.MainAxisSize = MainAxisSize;
        renderObject.CrossAxisAlignment = CrossAxisAlignment;
        renderObject.VerticalDirection = VerticalDirection;
    }
}

/// <summary>
/// 固定された軸方向で複数の子要素を一列に配置するウィジェットの基底型です。
/// </summary>
/// <param name="Direction">子要素を並べる主軸の方向。</param>
/// <seealso cref="Flex"/>
/// <seealso cref="RenderFlex"/>
public abstract record FlexWrapper(Axis Direction) : StatelessWidget
{
    /// <summary>
    /// 主軸に沿って配置する子ウィジェットの一覧を取得します。
    /// </summary>
    public List<Widget> Children { get; init; } = [];
    /// <summary>
    /// 主軸方向における子要素の配置方法を取得します。
    /// </summary>
    public MainAxisAlignment MainAxisAlignment { get; init; } = MainAxisAlignment.Start;
    /// <summary>
    /// 交差軸方向における子要素の配置方法を取得します。
    /// </summary>
    public CrossAxisAlignment CrossAxisAlignment { get; init; } = CrossAxisAlignment.Start;
    /// <summary>
    /// 主軸方向に確保する領域の大きさを取得します。
    /// </summary>
    public MainAxisSize MainAxisSize { get; init; } = MainAxisSize.Max;

    /// <summary>
    /// 固定された軸方向と現在の配置設定を使用する子ウィジェットを構築します。
    /// </summary>
    /// <param name="context">このウィジェットが配置されている構築コンテキスト。</param>
    /// <returns>現在の子要素と配置設定を保持する<see cref="Flex"/>。</returns>
    public override Widget Build(IBuildContext context)
    {
        return new Flex
        {
            MainAxisAlignment = MainAxisAlignment,
            CrossAxisAlignment = CrossAxisAlignment,
            MainAxisSize = MainAxisSize,
            Direction = Direction,
            Children = Children,
        };
    }
}

/// <summary>
/// 複数の子要素を垂直方向に一列に配置します。
/// </summary>
/// <seealso cref="Flex"/>
/// <seealso cref="RenderFlex"/>
public sealed record Column() : FlexWrapper(Axis.Vertical);

/// <summary>
/// 複数の子要素を水平方向に一列に配置します。
/// </summary>
/// <seealso cref="Flex"/>
/// <seealso cref="RenderFlex"/>
public sealed record Row() : FlexWrapper(Axis.Horizontal);