using FloatSoda.Core;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// 画像プロバイダーから読み込んだ画像を自身の領域へ描画します。
/// </summary>
/// <seealso cref="RenderImage"/>
public record Image : SingleChildRenderObjectWidget<RenderImage>
{
    /// <summary>
    /// 描画する画像を読み込むプロバイダーを取得します。
    /// </summary>
    public required ImageProvider ImageProvider { get; init; }

    /// <summary>
    /// プロバイダーから画像を読み込み、その画像を描画するRenderObjectを生成します。
    /// </summary>
    /// <returns>読み込んだ画像を所有する新しいRenderObject。</returns>
    /// <remarks>このメソッドの呼び出し中に画像の読み込みを実行します。</remarks>
    public override RenderImage CreateRenderObject()
    {
        return new RenderImage()
        {
            Image = ImageProvider.Load()
        };
    }
}