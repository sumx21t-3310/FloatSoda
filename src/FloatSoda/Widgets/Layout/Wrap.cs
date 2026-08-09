using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>複数の子要素を run へ分割し、利用可能な主軸領域で折り返して配置します。</summary>
/// <seealso cref="RenderWrap"/>
public sealed record Wrap : MultiChildRenderObjectWidget<RenderWrap>
{
    /// <summary>子要素を並べる主軸の方向を取得します。</summary>
    public Axis Direction
    {
        get;
        init
        {
            EnsureDefined(value, nameof(Direction));
            field = value;
        }
    } = Axis.Horizontal;

    /// <summary>各 run 内で子要素を主軸方向へ配置する方法を取得します。</summary>
    public WrapAlignment Alignment
    {
        get;
        init
        {
            EnsureDefined(value, nameof(Alignment));
            field = value;
        }
    } = WrapAlignment.Start;

    /// <summary>同じ run にある子要素同士の最小間隔を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または有限値ではありません。</exception>
    public double Spacing
    {
        get;
        init
        {
            EnsureFiniteAndNonNegative(value, nameof(Spacing));
            field = value;
        }
    }

    /// <summary>run 全体を交差軸方向へ配置する方法を取得します。</summary>
    public WrapAlignment RunAlignment
    {
        get;
        init
        {
            EnsureDefined(value, nameof(RunAlignment));
            field = value;
        }
    } = WrapAlignment.Start;

    /// <summary>隣接する run 同士の最小間隔を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または有限値ではありません。</exception>
    public double RunSpacing
    {
        get;
        init
        {
            EnsureFiniteAndNonNegative(value, nameof(RunSpacing));
            field = value;
        }
    }

    /// <summary>各 run 内で子要素を交差軸方向へ配置する方法を取得します。</summary>
    public WrapCrossAlignment CrossAxisAlignment
    {
        get;
        init
        {
            EnsureDefined(value, nameof(CrossAxisAlignment));
            field = value;
        }
    } = WrapCrossAlignment.Start;

    /// <summary>垂直方向における開始側と終了側の向きを取得します。</summary>
    public VerticalDirection VerticalDirection
    {
        get;
        init
        {
            EnsureDefined(value, nameof(VerticalDirection));
            field = value;
        }
    } = VerticalDirection.Down;

    /// <inheritdoc/>
    public override RenderWrap CreateRenderObject() => new()
    {
        Direction = Direction,
        Alignment = Alignment,
        Spacing = Spacing,
        RunAlignment = RunAlignment,
        RunSpacing = RunSpacing,
        CrossAxisAlignment = CrossAxisAlignment,
        VerticalDirection = VerticalDirection,
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderWrap renderObject)
    {
        renderObject.Direction = Direction;
        renderObject.Alignment = Alignment;
        renderObject.Spacing = Spacing;
        renderObject.RunAlignment = RunAlignment;
        renderObject.RunSpacing = RunSpacing;
        renderObject.CrossAxisAlignment = CrossAxisAlignment;
        renderObject.VerticalDirection = VerticalDirection;
    }

    private static void EnsureFiniteAndNonNegative(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "間隔には0以上の有限値を指定してください。");
        }
    }

    private static void EnsureDefined<T>(T value, string parameterName) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "定義済みの値を指定してください。");
        }
    }
}
