using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>子の自然な最大高さに合わせて高さを決めます。</summary>
/// <remarks>
/// intrinsic測定は追加のレイアウト走査を行い、入れ子では最悪O(N²)になり得ます。
/// スクロール領域や大規模ツリーでは使用せず、可能なら<see cref="SizedBox"/>や<see cref="ConstrainedBox"/>を使用してください。
/// </remarks>
public sealed record IntrinsicHeight : SingleChildRenderObjectWidget<RenderIntrinsicHeight>
{
    /// <summary>自然な高さを切り上げる単位を取得します。</summary>
    /// <value>正の有限値。丸めない場合は<see langword="null"/>です。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下または非有限値です。</exception>
    public double? StepHeight
    {
        get;
        init
        {
            RenderIntrinsicWidth.ValidateStep(value, nameof(StepHeight));
            field = value;
        }
    }

    /// <inheritdoc/>
    public override RenderIntrinsicHeight CreateRenderObject() => new() { StepHeight = StepHeight };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderIntrinsicHeight renderObject) => renderObject.StepHeight = StepHeight;
}
