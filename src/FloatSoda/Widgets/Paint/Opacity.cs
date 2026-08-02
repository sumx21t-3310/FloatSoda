using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// 子要素を指定した不透明度で合成します。
/// </summary>
/// <remarks>
/// 値が0または1の場合は中間レイヤーを作成しない高速経路を使用します。
/// </remarks>
/// <seealso cref="RenderOpacity"/>
public record Opacity : SingleChildRenderObjectWidget<RenderOpacity>
{
    /// <summary>
    /// 子要素へ適用する0から1までの不透明度を取得します。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 値が0から1の範囲外、または有限値ではありません。
    /// </exception>
    public double Value
    {
        get;
        init
        {
            if (!double.IsFinite(value) || value is < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "不透明度には0から1までの有限値を指定してください。");
            }

            field = value;
        }
    } = 1;

    /// <inheritdoc/>
    public override RenderOpacity CreateRenderObject() => new() { Opacity = Value };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderOpacity renderObject) => renderObject.Opacity = Value;
}
