using FloatSoda.Rendering.Layers;

namespace FloatSoda.Rendering;

/// <summary>
/// Layerツリーのレイアウトと描画を実行します。
/// 描画先のクリアやフラッシュ、Surfaceの所有権は呼び出し側が管理します。
/// </summary>
public static class LayerRenderer
{
    public static void Render(ILayer root, LayerContext context)
    {
        ArgumentNullException.ThrowIfNull(root);

        root.Layout(context);
        root.Paint(context);
    }
}
