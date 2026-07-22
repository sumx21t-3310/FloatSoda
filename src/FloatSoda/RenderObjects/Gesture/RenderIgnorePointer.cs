using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Gesture;

/// <summary>
/// 子要素の描画とレイアウトを維持したまま、自身を含む部分木をポインターのヒットテストから除外できるRenderObjectです。
/// </summary>
/// <seealso cref="Widgets.Gesture.IgnorePointer"/>
public class RenderIgnorePointer : RenderProxyBox
{
    /// <summary>自身を含む部分木へのポインター入力を無視するかどうかを取得または設定します。</summary>
    /// <remarks>変更は次回のヒットテストから反映され、Layout DirtyまたはPaint Dirtyにはしません。</remarks>
    public bool Ignoring { get; set; }

    /// <summary>無視中は部分木を探索せず、常にヒットしなかったものとして扱います。</summary>
    /// <param name="result">ヒットした対象を格納する結果。</param>
    /// <param name="position">このRenderObjectのローカル座標で表した判定位置。</param>
    /// <returns>無視中は常に<see langword="false"/>、それ以外は基底実装がヒットした場合に<see langword="true"/>。</returns>
    public override bool HitTest(HitTestResult result, Offset position) => !Ignoring && base.HitTest(result, position);
}
