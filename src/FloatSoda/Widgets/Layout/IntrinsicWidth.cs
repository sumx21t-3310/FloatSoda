using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>子の自然な最大幅に合わせて幅を決めます。</summary>
/// <remarks>
/// intrinsic測定は追加のレイアウト走査を行い、入れ子では最悪O(N²)になり得ます。
/// スクロール領域や大規模ツリーでは使用せず、可能なら<see cref="SizedBox"/>や<see cref="ConstrainedBox"/>を使用してください。
/// </remarks>
public sealed record IntrinsicWidth : SingleChildRenderObjectWidget<RenderIntrinsicWidth>
{
    /// <summary>自然な幅を切り上げる単位を取得します。</summary>
    /// <value>正の有限値。丸めない場合は<see langword="null"/>です。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下または非有限値です。</exception>
    public double? StepWidth
    {
        get;
        init
        {
            RenderIntrinsicWidth.ValidateStep(value, nameof(StepWidth));
            field = value;
        }
    }

    /// <inheritdoc/>
    public override RenderIntrinsicWidth CreateRenderObject() => new() { StepWidth = StepWidth };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderIntrinsicWidth renderObject) => renderObject.StepWidth = StepWidth;
}
