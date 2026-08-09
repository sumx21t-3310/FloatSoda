using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>幅と高さが指定した比率になるように子要素をレイアウトします。</summary>
/// <seealso cref="RenderAspectRatio"/>
public sealed record AspectRatio : SingleChildRenderObjectWidget<RenderAspectRatio>
{
    /// <summary>幅を高さで割った比率を取得します。</summary>
    /// <value>正の有限値。16:9の場合は<c>16.0 / 9.0</c>です。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下または有限値ではありません。</exception>
    public required double Ratio
    {
        get;
        init
        {
            RenderAspectRatio.ValidateAspectRatio(value, nameof(Ratio));
            field = value;
        }
    }

    /// <inheritdoc/>
    public override RenderAspectRatio CreateRenderObject() => new() { AspectRatio = Ratio };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderAspectRatio renderObject) =>
        renderObject.AspectRatio = Ratio;
}
