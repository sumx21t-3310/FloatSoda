using FloatSoda.Rendering.Layers;

namespace FloatSoda.Rendering;

/// <summary>
/// Layerツリーのレイアウトと描画を実行します。
/// 描画先のクリアやフラッシュ、Surfaceの所有権は呼び出し側が管理します。
/// </summary>
public static class LayerRenderer
{
    /// <summary>
    /// レイヤーツリーのレイアウトを完了してから、指定された描画先へツリーを合成します。
    /// </summary>
    /// <param name="root">
    /// 描画するレイヤーツリーのルート。<see langword="null"/>は指定できません。
    /// 所有権は呼び出し元に残ります。
    /// </param>
    /// <param name="context">
    /// 描画先のキャンバスを保持するコンテキスト。キャンバスの所有権は呼び出し元に残ります。
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="root"/>が<see langword="null"/>の場合にスローされます。
    /// </exception>
    /// <remarks>
    /// 描画先のクリア、フラッシュ、および破棄は行いません。
    /// レイアウトにより、ツリー内の各レイヤーの描画境界が更新されます。
    /// </remarks>
    /// <seealso cref="LayerBitmapRenderer"/>
    public static void Render(ILayer root, LayerContext context)
    {
        ArgumentNullException.ThrowIfNull(root);

        root.Layout(context);
        root.Paint(context);
    }
}
