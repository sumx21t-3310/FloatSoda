using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 親から受け取った制約を任意に変換して子へ渡し、親制約内で子を配置します。
/// </summary>
/// <seealso cref="RenderConstraintsTransformBox"/>
public record ConstraintsTransformBox : SingleChildRenderObjectWidget<RenderConstraintsTransformBox>
{
    /// <summary>子へ渡す制約を生成する変換を取得します。</summary>
    /// <exception cref="ArgumentNullException">値が<see langword="null"/>です。</exception>
    public required BoxConstraintsTransform ConstraintsTransform
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>自身と子のサイズが異なる場合に使用する配置を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が非有限値です。</exception>
    public Alignment Alignment
    {
        get;
        init
        {
            if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "配置値には有限値を指定してください。");
            }

            field = value;
        }
    } = Alignment.Center;

    /// <summary>子が自身の領域からはみ出す場合の切り抜き方法を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public Clip ClipBehavior
    {
        get;
        init
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "定義済みのクリップ方法を指定してください。");
            }

            field = value;
        }
    } = Clip.None;

    /// <inheritdoc/>
    public override RenderConstraintsTransformBox CreateRenderObject() => new()
    {
        ConstraintsTransform = ConstraintsTransform,
        Alignment = Alignment,
        ClipBehavior = ClipBehavior
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderConstraintsTransformBox renderObject)
    {
        renderObject.ConstraintsTransform = ConstraintsTransform;
        renderObject.Alignment = Alignment;
        renderObject.ClipBehavior = ClipBehavior;
    }

    /// <summary>受け取った制約を変更せずに返します。</summary>
    /// <param name="constraints">親から受け取った制約。</param>
    /// <returns><paramref name="constraints"/>と同じ制約。</returns>
    public static BoxConstraints Unmodified(BoxConstraints constraints) => constraints;

    /// <summary>幅と高さの制約をすべて取り除きます。</summary>
    /// <param name="constraints">親から受け取った制約。この変換では使用しません。</param>
    /// <returns>両軸の最小値が0、最大値が正の無限大の制約。</returns>
    public static BoxConstraints Unconstrained(BoxConstraints constraints) => BoxConstraints.Unbounded;

    /// <summary>幅の最小値と最大値を取り除き、高さの制約を維持します。</summary>
    /// <param name="constraints">親から受け取った制約。</param>
    /// <returns>幅だけが制約されていない制約。</returns>
    public static BoxConstraints WidthUnconstrained(BoxConstraints constraints) => constraints with
    {
        MinWidth = 0,
        MaxWidth = double.PositiveInfinity
    };

    /// <summary>高さの最小値と最大値を取り除き、幅の制約を維持します。</summary>
    /// <param name="constraints">親から受け取った制約。</param>
    /// <returns>高さだけが制約されていない制約。</returns>
    public static BoxConstraints HeightUnconstrained(BoxConstraints constraints) => constraints with
    {
        MinHeight = 0,
        MaxHeight = double.PositiveInfinity
    };

    /// <summary>最大幅だけを取り除き、最小幅と高さの制約を維持します。</summary>
    /// <param name="constraints">親から受け取った制約。</param>
    /// <returns>最大幅が正の無限大の制約。</returns>
    public static BoxConstraints MaxWidthUnconstrained(BoxConstraints constraints) => constraints with
    {
        MaxWidth = double.PositiveInfinity
    };

    /// <summary>最大高さだけを取り除き、最小高さと幅の制約を維持します。</summary>
    /// <param name="constraints">親から受け取った制約。</param>
    /// <returns>最大高さが正の無限大の制約。</returns>
    public static BoxConstraints MaxHeightUnconstrained(BoxConstraints constraints) => constraints with
    {
        MaxHeight = double.PositiveInfinity
    };

    /// <summary>最大幅と最大高さを取り除き、両軸の最小値を維持します。</summary>
    /// <param name="constraints">親から受け取った制約。</param>
    /// <returns>両軸の最大値が正の無限大の制約。</returns>
    public static BoxConstraints MaxUnconstrained(BoxConstraints constraints) => constraints with
    {
        MaxWidth = double.PositiveInfinity,
        MaxHeight = double.PositiveInfinity
    };
}

/// <summary>
/// 指定した軸以外の親制約を取り除き、子を自然な大きさでレイアウトします。
/// </summary>
/// <seealso cref="ConstraintsTransformBox"/>
public sealed record UnconstrainedBox : StatelessWidget
{
    /// <summary>制約を維持する軸を取得します。<see langword="null"/>の場合は両軸の制約を取り除きます。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public Axis? ConstrainedAxis
    {
        get;
        init
        {
            if (value is { } axis && !Enum.IsDefined(axis))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "定義済みの軸を指定してください。");
            }

            field = value;
        }
    }

    /// <summary>自身と子のサイズが異なる場合に使用する配置を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が非有限値です。</exception>
    public Alignment Alignment
    {
        get;
        init
        {
            if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "配置値には有限値を指定してください。");
            }

            field = value;
        }
    } = Alignment.Center;

    /// <summary>子が自身の領域からはみ出す場合の切り抜き方法を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public Clip ClipBehavior
    {
        get;
        init
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "定義済みのクリップ方法を指定してください。");
            }

            field = value;
        }
    } = Clip.None;

    /// <summary>制約を取り除いて子を配置するウィジェットを構築します。</summary>
    /// <param name="context">このウィジェットが配置されている構築コンテキスト。</param>
    /// <returns>軸に対応した標準変換を持つ<see cref="ConstraintsTransformBox"/>。</returns>
    public override Widget Build(IBuildContext context) => new ConstraintsTransformBox
    {
        ConstraintsTransform = ConstrainedAxis switch
        {
            Axis.Horizontal => ConstraintsTransformBox.HeightUnconstrained,
            Axis.Vertical => ConstraintsTransformBox.WidthUnconstrained,
            null => ConstraintsTransformBox.Unconstrained,
            _ => throw new ArgumentOutOfRangeException(nameof(ConstrainedAxis), ConstrainedAxis, null)
        },
        Alignment = Alignment,
        ClipBehavior = ClipBehavior,
        Child = Child
    };

    /// <summary>制約を取り除いて配置する子ウィジェットを取得します。</summary>
    public Widget? Child { get; init; }
}
