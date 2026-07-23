using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.RenderObjects;

/// <summary>親RenderObjectが子ごとに保持する配置情報を表すインターフェースです。</summary>
public interface IParentData;

/// <summary>ボックスレイアウト内で子の配置位置を保持します。</summary>
/// <param name="offset">親の座標系における子の初期位置。</param>
public class BoxParentData(Offset offset = default) : IParentData
{
    /// <summary>親の座標系における子の配置位置を取得または設定します。</summary>
    /// <value>親の描画原点から子の描画原点までの移動量。</value>
    /// <remarks>
    /// この値自体はDirty状態を変更しません。
    /// 親は値を変更したレイアウト処理と同じパイプライン更新内で、後続の描画に新しい位置を使用します。
    /// </remarks>
    public Offset Offset { get; set; } = offset;
}
